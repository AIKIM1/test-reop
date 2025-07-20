/*************************************************************************************
 Created Date : 2022.08.17
      Creator : 이제섭
   Decription : 장기보류 셀 등록
--------------------------------------------------------------------------------------
 [Change History]
  2022.08.17  이제섭 : Initial Created.
  2022.08.30  이제섭 : Tray 구성 탭 신규 추가

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using Microsoft.Win32;
using System.Configuration;
using C1.WPF.Excel;
using System.Text;
using System.Linq;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// 
    /// </summary>
    public partial class FCS001_138 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string gsProcessType = string.Empty;
        private string gsEqpKind = string.Empty;
        private string sCalTime = string.Empty;
        private string _TrayType = string.Empty;

        private bool bGood = true; // 화면 Load 시 Default 선택이 양품화 라디오버튼임.
        private bool bDefect = false;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        Util _Util = new Util();

        public FCS001_138()
        {
            InitializeComponent();
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(gsEqpKind))
                gsEqpKind = GetLaneIDForMenu(LoginInfo.CFG_MENUID);

            switch (gsEqpKind)
            {
                case "D": //DEGAS
                    //gsEqpKind = "D";
                    gsProcessType = "B";
                    break;

                case "5": //특성기
                    //gsEqpKind = "5";
                    gsProcessType = "D";
                    break;

                case "6": //SELECTOR
                    //gsEqpKind = "6";
                    gsProcessType = "A,C";
                    break;

                case "J": //JIG
                    //gsEqpKind = "J";
                    gsProcessType = "E";
                    break;

                default:
                    gsProcessType = "B";
                    gsEqpKind = "D";
                    break;
            }

            InitControl();

            Loaded -= UserControl_Loaded;
        }

        private void InitControl()
        {
            dtpFromTimeT1.DateTime = DateTime.Now.AddDays(-1);
            dgSearch.AddCheckAll();

            InitCombo();
        }

        private void InitCombo()
        {
            string[] sFilterEqp = { gsEqpKind, null };
            string[] sFilterSearchShift = { "STM", null, null, null, null, null };
            string[] sFilterDefect = { gsEqpKind };
            string[] sFilterSearchDefect = { null, gsEqpKind };
            string[] sFilterOp = { "CLO", "A,B,C,D" };
            string[] sFilterEqpInput = { "EQP_INPUT_YN" };

            CommonCombo_Form _combo = new CommonCombo_Form();

            //C1ComboBox[] cboLineChild = { cboModel };

            if (gsEqpKind.Equals("5") || gsEqpKind.Equals("D"))   //Degas EOL만 설비 Display
            {
                C1ComboBox[] cboSearchLaneChild = { cboSearchEqp };
                _combo.SetCombo(cboSearchLane, CommonCombo_Form.ComboStatus.ALL, cbParent: cboSearchLaneChild, cbChild: cboSearchLaneChild, sCase: "LANE");

                C1ComboBox[] cboSearchEqpParent = { cboSearchLane };
                _combo.SetCombo(cboSearchEqp, CommonCombo_Form.ComboStatus.ALL, cbParent: cboSearchEqpParent, sFilter: sFilterEqp, sCase: "EQPIDBYLANE");

                C1ComboBox[] cboHistLaneChild = { cboHistEqp };
                _combo.SetCombo(cboHistLane, CommonCombo_Form.ComboStatus.ALL, cbParent: cboHistLaneChild, cbChild: cboHistLaneChild, sCase: "LANE");

                C1ComboBox[] cboHistEqpParent = { cboHistLane };
                _combo.SetCombo(cboHistEqp, CommonCombo_Form.ComboStatus.ALL, cbParent: cboHistEqpParent, sFilter: sFilterEqp, sCase: "EQPIDBYLANE");
            }

            //C1ComboBox[] cboDefectParent = { cboInsertGroup };
            //_combo.SetCombo(cboInsertDefect, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilterDefect, sCase: "CELL_DEFECT", cbParent: cboDefectParent);
            //C1ComboBox[] cboGroupChild = { cboInsertDefect };
            //_combo.SetCombo(cboInsertGroup, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilterDefect, sCase: "DEFECT_KIND", cbChild: cboGroupChild);

            _combo.SetCombo(cboSearchModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL");

            string[] arrColumnShift = { "LANGID", "AREAID" };
            string[] arrConditionShift = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            cboSearchShift.SetDataComboItem("DA_BAS_SEL_COMBO_FORM_SHIFT_LIST", arrColumnShift, arrConditionShift, CommonCombo.ComboStatus.ALL, true);

            string[] arrColumn = { "LANGID", "USE_FLAG", "DFCT_TYPE_CODE", "DFCT_GR_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, "Y", null, gsEqpKind };
            //cboSearchDefect.SelectedValuePath = "DFCT_CODE"; cboSearchDefect.DisplayMemberPath = "DFCT_NAME";
            //cboSearchDefect.SetDataComboItem("DA_BAS_SEL_COMBO_TM_CELL_DEFECT", arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, true);

            cboSearchOp.SetCommonCode("FORM_DFCT_GR_TYPE_CODE", CommonCombo.ComboStatus.SELECT, true);
            cboEqpInputYN.SetCommonCode("EQPT_INPUT_AVAIL_FLAG", CommonCombo.ComboStatus.ALL, true);

            //_combo.SetCombo(cboHistShift, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilterSearchShift, sCase: "CMN_WITH_OPTION");
            _combo.SetCombo(cboHistModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL");
            //_combo.SetCombo(cboHistDefect, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilterSearchDefect, sCase: "CELL_DEFECT");
            //_combo.SetCombo(cboHistOp, CommonCombo_Form.ComboStatus.SELECT, sFilter: sFilterOp, sCase: "CMN");

            //string[] sFilterYN = { "USE_YN" };
            //_combo.SetCombo(cboSelectYN, CommonCombo_Form.ComboStatus.NONE, sFilter: sFilterYN, sCase: "CMN", bCodeDisplay: true);

            // 복구 이력 탭의 콤보박스 설정
            cboHistOp.SetCommonCode("FORM_DFCT_GR_TYPE_CODE", CommonCombo.ComboStatus.SELECT, true);

            //동
            C1ComboBox[] cboAreaChild = { cboDummyLineID };
            _combo.SetCombo(cboArea, CommonCombo_Form.ComboStatus.ALL, cbChild: cboAreaChild);

            //Login 한 AREA Setting
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;

            C1ComboBox[] cboLineParent = { cboArea };
            _combo.SetCombo(cboDummyLineID, CommonCombo_Form.ComboStatus.NONE, sCase: "LINE", cbParent: cboLineParent);

            string[] sFilter = { "SPCL_FLAG" };
            _combo.SetCombo(cboSpecial, CommonCombo_Form.ComboStatus.NONE, sFilter: sFilter, sCase: "CMN");

            //string[] arrColumnHistShift = { "LANGID", "AREAID" };
            //string[] arrConditionHistShift = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            //cboHistShift.SetDataComboItem("DA_BAS_SEL_COMBO_FORM_SHIFT_LIST", arrColumnHistShift, arrConditionHistShift, CommonCombo.ComboStatus.ALL, true);

        }

        private void CalDate()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("SYSTIME", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SYSTIME"] = DateTime.Today.ToString("yyyy-MM-dd");

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORM_SHFT_ID_BY_SYSTIME", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    sCalTime = dtRslt.Rows[0]["SHFT_END_HMS"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private string GetLaneIDForMenu(string sMenuID)
        {
            string sLaneID = string.Empty;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_CHARGE_MENU_ID";
                dr["CMCODE"] = sMenuID;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt.Rows.Count > 0)
                {
                    sLaneID = dtRslt.Rows[0]["ATTRIBUTE1"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return sLaneID;
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //fpsInsert.ActiveSheet.Columns["CELL_ID"].Locked = true;
            GetInsertCellDataFromSpread(dgInputList, dgInputList, dgInputList.Columns["SUBLOTID"].Index);
        }

        private void GetInsertCellDataFromSpread(C1DataGrid fromSpread, C1DataGrid toSpread, int indexCellCol)
        {
            string sCellID = string.Empty;

            for (int iRow = 0; iRow < fromSpread.GetRowCount(); iRow++)
            {

                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(fromSpread.Rows[iRow].DataItem, "SUBLOTID"))))
                    continue;
                sCellID += fromSpread.GetCell(iRow, indexCellCol).Text.ToString() + ",";
            }

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("SUBLOTID", typeof(string));
            dtRqst.Columns.Add("EQP_KIND", typeof(string));
            dtRqst.Columns.Add("PROCESS_TYPE", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["SUBLOTID"] = sCellID;
            dr["EQP_KIND"] = gsEqpKind;
            dr["PROCESS_TYPE"] = gsProcessType;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SUBLOT_INFO_FOR_LONG_TERM_HOLD", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);
            Util.GridSetData(toSpread, dtRslt, FrameOperation, false);

            txtBadInsertRow.Text = dtRslt.Select("CHK = '0'").Length.ToString();

            for (int i = 0; i < toSpread.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(toSpread.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    toSpread.Rows[i].Presenter.Tag = "U";
                    //toSpread.ActiveSheet.Cells[i, toSpread.GetColumnIndex("CHK")].Locked = false;

                    if (gsEqpKind.Equals("D") || gsEqpKind.Equals("5"))
                    {
                        DataTableConverter.SetValue(toSpread.Rows[i].DataItem, "EQPTNAME", Util.NVC(DataTableConverter.GetValue(toSpread.Rows[i].DataItem, "EQPTID")));

                    }
                }
                else
                {
                    //toSpread.ActiveSheet.Rows[i].Tag = null;
                    //toSpread.ActiveSheet.Cells[i, toSpread.GetColumnIndex("CHK")].Locked = true;
                }
            }
            txtInsertCellCnt.Text = dtRslt.Rows.Count.ToString();
            toSpread.Refresh();
        }

        public static DataTable GetLossEqp(string sLaneId, string sEqpType, bool bCodeDisplay = false)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("EQPTTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = sLaneId;
                dr["EQPTTYPE"] = sEqpType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_EQPT_IN_EQPTTYPE", "RQSTDT", "RSLTDT", RQSTDT);

                return SetCodeDisplay(dtResult, bCodeDisplay);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        public static DataTable SetCodeDisplay(DataTable dt, bool bCodeDisplay)
        {
            if (bCodeDisplay)
            {
                foreach (DataRow drRslt in dt.Rows)
                {
                    drRslt["CBO_NAME"] = "[" + drRslt["CBO_CODE"].ToString() + "]" + drRslt["CBO_NAME"].ToString();
                }
            }
            return dt;
        }
        private void btnHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(dgInputList.ItemsSource);

                if (dt.Columns.Count < 2)
                {
                    // 조회된 값이 없습니다.
                    Util.MessageValidation("FM_ME_0232");
                    return;
                }
                if (!dgInputList.IsCheckedRow("CHK"))
                {
                    // 선택된 Cell ID가 없습니다.
                    Util.MessageValidation("FM_ME_0161");
                    return;
                }

                DateTime dateReg = new DateTime(dtpFromDateT01.SelectedDateTime.Year,
                                                dtpFromDateT01.SelectedDateTime.Month,
                                                dtpFromDateT01.SelectedDateTime.Day,
                                                dtpFromTimeT1.DateTime.Value.Hour,
                                                dtpFromTimeT1.DateTime.Value.Minute,
                                                dtpFromTimeT1.DateTime.Value.Second);

                //for (int i = 0; i < dgInputList.Rows.Count; i++)
                //{
                //    if (dgInputList.GetStringValue(i, "WORK_DATE").Equals(string.Empty)) continue;

                //    if (dateReg.AddDays(-1) > Convert.ToDateTime(dgInputList.GetStringValue(i, "WORK_DATE")) ||
                //        dateReg.AddDays(1) < Convert.ToDateTime(dgInputList.GetStringValue(i, "WORK_DATE")))
                //    {
                //        //등록하려는 작업일자가 %1의 작업일자와 하루 이상 차이나서 등록불가.
                //        Util.MessageValidation("FM_ME_0438", dgInputList.GetStringValue(i, "SUBLOTID"));
                //        return;
                //    }
                //}

                //CalDate();

                //DateTime dCalDate = DateTime.ParseExact(DateTime.Today.ToString("yyyyMMdd") + sCalTime, "yyyyMMddHHmmss", null);
                //DateTime dJobDate = DateTime.ParseExact((Util.GetCondition(dtpFromDateT01, bAllNull: true).ToString() + dtpFromTimeT1.DateTime.Value.ToString("HHmmss").ToString()), "yyyyMMddHHmmss", null);

                //if (DateTime.Compare(DateTime.Now, dJobDate) == -1)
                //{
                //    //등록일 기준 미래일자는 등록할 수 없습니다. 
                //    Util.Alert("FM_ME_0443");
                //    return;
                //}
                //else
                //{

                //    if ((DateTime.Now - dCalDate).TotalDays > 1)
                //    {
                //        //작업마감시간 기준, 하루를 초과한 경우 등록할 수 없습니다. 
                //        Util.Alert("FM_ME_0444");
                //        return;
                //    }

                //    if (dCalDate.Month != DateTime.Now.Month)
                //    {
                //        // 작업마감시간과 등록일의 달이 다른 경우 등록할 수 없습니다.
                //        Util.Alert("FM_ME_0445");
                //        return;
                //    }

                //    if (dJobDate <= dCalDate.AddDays(-1))
                //    {
                //        // 작업마감시간 이전 시간은 등록할 수 없습니다. 
                //        Util.Alert("FM_ME_0446");
                //        return;
                //    }
                //}


                if (DateTime.Now < dateReg)
                {
                    //등록일 기준 미래일자는 등록할 수 없습니다. 
                    Util.MessageValidation("FM_ME_0443");
                    return;
                }

                //ERP 마감여부 체크
                if (!ChkErpClosingFlag(dateReg))
                    return;



                //저장하시겠습니까?
                Util.MessageConfirm("FM_ME_0214", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        if (dgInputList.GetRowCount() <= 0)
                        {
                            //등록할 대상이 존재하지 않습니다.
                            Util.MessageValidation("FM_ME_0125");
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(txtRemark.Text))
                        {
                            // 사유를 입력하세요
                            Util.MessageValidation("SFU1594");
                            return;
                        }

                        DataSet dsInDataSet = new DataSet();

                        DataTable dtINDATA = new DataTable();
                        dtINDATA.TableName = "INDATA";
                        dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                        dtINDATA.Columns.Add("IFMODE", typeof(string));
                        dtINDATA.Columns.Add("USERID", typeof(string));
                        dtINDATA.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));
                        dtINDATA.Columns.Add("REMARKS_CNTT", typeof(string));
                        dtINDATA.Columns.Add("CALDATE", typeof(DateTime));
                        dsInDataSet.Tables.Add(dtINDATA);

                        DataRow drInData = dtINDATA.NewRow();
                        drInData["SRCTYPE"] = "UI";
                        drInData["IFMODE"] = "OFF";
                        drInData["USERID"] = LoginInfo.USERID;
                        drInData["LOT_DETL_TYPE_CODE"] = "H";
                        drInData["REMARKS_CNTT"] = txtRemark.Text;
                        drInData["CALDATE"] = dateReg;
                        dtINDATA.Rows.Add(drInData);

                        DataTable dtIN_SUBLOT = new DataTable();
                        dtIN_SUBLOT.TableName = "IN_SUBLOT";
                        dtIN_SUBLOT.Columns.Add("SUBLOTID", typeof(string));
                        dsInDataSet.Tables.Add(dtIN_SUBLOT);

                        for (int i = 0; i < dgInputList.Rows.Count; i++)
                        {
                            if (dgInputList.IsCheckedRow("CHK", i)
                                && string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[i].DataItem, "DENY_DESC"))))
                            {
                                DataRow drInSublot = dtIN_SUBLOT.NewRow();
                                drInSublot["SUBLOTID"] = dgInputList.GetValue(i, "SUBLOTID");
                                dtIN_SUBLOT.Rows.Add(drInSublot);

                                try
                                {
                                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_SUBLOT_TRANSFER_HOLD_LOT", "INDATA,IN_SUBLOT", "OUTDATA", dsInDataSet, FrameOperation.MENUID);

                                    if (dsResult.Tables[0].Rows[0]["RETVAL"].ToString().Equals("0"))
                                    {
                                        dgInputList.SetValue(i, "DENY_DESC", "SUCCESS");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    dgInputList.SetValue(i, "DENY_DESC", "FAIL");
                                }
                                finally
                                {
                                    //dtIN_SUBLOT.Clear();
                                }
                            }
                        }

                        // 비고란을 통해 성공여부를 확인할 수 있습니다.
                        Util.MessageInfo("FM_ME_0448");
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringBuilder cellIDs = new StringBuilder();

                DataTable dtRslt = new DataTable();
                for (int iRow = 0; iRow < dgRecover.GetRowCount(); iRow++)
                {
                    int indexCellCol = dgRecover.Columns["SUBLOTID"].Index;
                    string sCellID = string.Empty;
                    string sTemp = Util.NVC(DataTableConverter.GetValue(dgRecover.Rows[iRow].DataItem, "SUBLOTID"));
                    if (sTemp.Trim() == string.Empty)
                        break;

                    sCellID = sTemp;

                    //스프레드에 있는지 확인
                    if (iRow != 0)
                    {
                        for (int i = 0; i <= iRow - 1; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgRecover.Rows[i].DataItem, "SUBLOTID")).Equals(sCellID))
                            {
                                Util.MessageInfo("FM_ME_0287", new string[] { sCellID });  //[CELL ID : {0}]목록에 기존재하는 Cell 입니다.
                                continue;
                            }
                        }
                    }

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("SUBLOTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["SUBLOTID"] = sCellID;
                    if (string.IsNullOrEmpty(dr["SUBLOTID"].ToString())) return;

                    dtRqst.Rows.Add(dr);

                    //Todo: 같은 Form에 두개이상의 메뉴를 사용할 경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
                    DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_SEL_HOLD_SUBLOT_FOR_RECOVER", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);
                    if (dtRslt1.Rows.Count > 0)
                    {
                        dtRslt.Merge(dtRslt1);
                    }
                    else
                    {
                        cellIDs.Append(sTemp + ", ");
                    }
                }

                if (dtRslt == null || dtRslt.Rows.Count == 0)
                {
                    dgRecover.ClearRows();
                }
                else
                {
                    Util.GridSetData(dgRecover, dtRslt, FrameOperation, true);
                }

                if (cellIDs.Length > 0)
                {
                    Util.MessageInfo(MessageDic.Instance.GetMessage("FM_ME_0035") + "\r\n\r\n" + cellIDs.ToString(0, cellIDs.Length - 2));  //Hold 대상이 없습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            dgInputList.ClearRows();
            txtInsertCellCnt.Text = "";
            txtBadInsertRow.Text = "";
            DataGridRowAdd(dgInputList, Convert.ToInt32(txtRowCntInsertCell.Text));
        }

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnRefresh2_Click(object sender, RoutedEventArgs e)
        {
            dgRecover.ClearRows();
            DataGridRowAdd(dgRecover, Convert.ToInt32(txtRowCntInsertCell2.Text));
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);
                        DataRow dr2 = dt.NewRow();
                        dt.Rows.Add(dr2);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < iRowcount; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnBadCellResotre_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("FM_ME_0147", (result) => //불량 Cell 복구를 하시겠습니까?
                {
                    DataSet dsInDataSet = new DataSet();
                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "INDATA";
                    dtRqst.Columns.Add("SRCTYPE", typeof(string));
                    dtRqst.Columns.Add("IFMODE", typeof(string));
                    dtRqst.Columns.Add("USERID", typeof(string));
                    dtRqst.Columns.Add("GOOD_YN", typeof(string));
                    dsInDataSet.Tables.Add(dtRqst);

                    DataTable dtRqst1 = new DataTable();
                    dtRqst1.TableName = "IN_SUBLOT";
                    dtRqst1.Columns.Add("SUBLOTID", typeof(string));
                    dsInDataSet.Tables.Add(dtRqst1);

                    DataRow dr = dtRqst.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["GOOD_YN"] = (bool)rdoGood.IsChecked ? "Y" : "N";
                    dtRqst.Rows.Add(dr);

                    for (int i = 0; i < dgRecover.GetRowCount(); i++)
                    {
                        DataRow dr1 = dtRqst1.NewRow();
                        dr1["SUBLOTID"] = dgRecover.GetValue(i, "SUBLOTID");
                        dtRqst1.Rows.Add(dr1);
                    }

                    if (dtRqst1.Rows.Count == 0)
                    {
                        Util.MessageInfo("FM_ME_0139");  //복구 대상이 없습니다.
                        return;
                    }

                    try
                    {
                        //Todo: 같은 Form에 두개이상의 메뉴를 사용할 경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_SUBLOT_HOLD_RESTORE", "INDATA,IN_SUBLOT", "OUTDATA", dsInDataSet, menuid: FrameOperation.MENUID);
                        if (!string.IsNullOrEmpty(dsRslt.Tables[0].Rows[0]["NO_CELL_ID"].ToString()) || dsRslt.Tables[0].Rows[0]["NO_CELL_ID"].ToString() != ",")
                        {
                            //NO_SUBLOTID String      불량항목 없어서 복구되지 않은 SUBLOTID LIST, 리턴 값 예시) AK21E12955,AK21E12956,AK21E12957
                            Util.MessageInfo("FM_ME_0366", new string[] { dsRslt.Tables[0].Rows[0]["NO_CELL_ID"].ToString() });  //{0}은 Cell의 등급이력이 없어 복구할 수 없습니다.
                        }

                        btnRefresh2_Click(null, null);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboSearchOp.GetBindValue() == null)
                {
                    Util.Alert("FM_ME_0107"); //공정을 선택해주세요.
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                if (rdoReg.IsChecked == true)
                {
                    dtRqst.Columns.Add("FROM_REG_DATE", typeof(string));
                    dtRqst.Columns.Add("TO_REG_DATE", typeof(string));
                }
                else if (rdoWork.IsChecked == true)
                {
                    dtRqst.Columns.Add("FROM_WORK_DATE", typeof(string));
                    dtRqst.Columns.Add("TO_WORK_DATE", typeof(string));
                }

                //dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("SHFT_ID", typeof(string));
                dtRqst.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("DFCT_CODE", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("S71", typeof(string));
                dtRqst.Columns.Add("EQPT_INPUT_AVAIL_FLAG", typeof(string));
                dtRqst.Columns.Add("REMARKS_CNTT", typeof(string));
                dtRqst.Columns.Add("UPDUSER", typeof(string));

                DataRow dr = dtRqst.NewRow();
                if (rdoReg.IsChecked == true)
                {
                    dr["FROM_REG_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpFromTime.DateTime.Value.ToString(" HH:mm:00");
                    dr["TO_REG_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpToTime.DateTime.Value.ToString(" HH:mm:00");
                }
                else if (rdoWork.IsChecked == true)
                {
                    dr["FROM_WORK_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd");
                    dr["TO_WORK_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd");
                }
                //dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SHFT_ID"] = cboSearchShift.GetBindValue();
                dr["DFCT_GR_TYPE_CODE"] = cboSearchOp.GetBindValue();
                //dr["DFCT_CODE"] = cboSearchDefect.GetBindValue();
                dr["SUBLOTID"] = txtSearchCellId.GetBindValue();
                if (cboSearchEqp.Visibility == Visibility.Visible)
                {
                    dr["S71"] = cboSearchLane.GetBindValue();
                    dr["EQPTID"] = cboSearchEqp.GetBindValue();
                }
                dr["MDLLOT_ID"] = cboSearchModel.GetBindValue();
                dr["PROD_LOTID"] = txtSearchLotId.GetBindValue();
                dr["EQPT_INPUT_AVAIL_FLAG"] = cboEqpInputYN.GetBindValue();
                //dr["REMARKS_CNTT"] = txtHoldDesc.GetBindValue();
                dr["UPDUSER"] = txtSearchRegUser.GetBindValue();
                dtRqst.Rows.Add(dr);

                //Todo: 같은 Form에 두개이상의 메뉴를 사용할 경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SUBLOT_TRANSFER_HOLD_LOT_HIST", "RQSTDT", "RSLTDT", dtRqst, gsEqpKind);

                // CHK 추가
                if (!dtRslt.Columns.Contains("CHK"))
                {
                    dtRslt.Columns.Add("CHK", typeof(bool));
                    dtRslt.Select().ToList().ForEach(row => row["CHK"] = false);
                    dtRslt.AcceptChanges();
                }

                Util.GridSetData(dgSearch, dtRslt, FrameOperation, true);

                if (!gsEqpKind.Equals("6")) // Selector
                {
                    for (int i = 0; i < dgSearch.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "EQPT_INPUT_AVAIL_FLAG")).Equals("N"))
                        {
                            DataTableConverter.SetValue(dgSearch.Rows[i].DataItem, "CHK", true);
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgSearch.Rows[i].DataItem, "CHK", false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearchRecover_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //불량 Cell 복구를 하시겠습니까?
                Util.MessageConfirm("FM_ME_0147", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Button btn = sender as Button;

                        bool bGood = false;
                        // 양품화 버튼 클릭 시 
                        if (btn == btnSearchRecoverGood)
                        {
                            bGood = true;
                        }

                        DataSet dsInDataSet = new DataSet();
                        DataTable dtINDATA = new DataTable();
                        dtINDATA.TableName = "INDATA";
                        dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                        dtINDATA.Columns.Add("IFMODE", typeof(string));
                        dtINDATA.Columns.Add("USERID", typeof(string));
                        dtINDATA.Columns.Add("GOOD_YN", typeof(string));
                        dsInDataSet.Tables.Add(dtINDATA);

                        DataRow dr = dtINDATA.NewRow();
                        dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        dr["IFMODE"] = IFMODE.IFMODE_OFF;
                        dr["USERID"] = LoginInfo.USERID;
                        dr["GOOD_YN"] = bGood ? "Y" : "N";
                        dtINDATA.Rows.Add(dr);

                        DataTable dtInSublot = new DataTable();
                        dtInSublot.TableName = "IN_SUBLOT";
                        dtInSublot.Columns.Add("SUBLOTID", typeof(string));
                        dsInDataSet.Tables.Add(dtInSublot);

                        for (int i = 0; i < dgSearch.Rows.Count; i++)
                        {
                            if (dgSearch.IsCheckedRow("CHK", i))
                            {
                                dtInSublot.Clear();

                                DataRow drInSublot = dtInSublot.NewRow();
                                drInSublot["SUBLOTID"] = dgSearch.GetValue(i, "SUBLOTID");
                                dtInSublot.Rows.Add(drInSublot);

                                try
                                {
                                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_SUBLOT_HOLD_RESTORE", "INDATA,IN_SUBLOT", "OUTDATA", dsInDataSet, menuid: FrameOperation.MENUID);

                                    if (dsRslt.Tables[0].Rows[0]["RETVAL"].ToString().Equals("0")) //0성공, -1실패
                                    {
                                        Util.MessageValidation("FM_ME_0140");  //저장하였습니다.
                                    }
                                    else
                                    {
                                        Util.MessageValidation("FM_ME_0311");  //저장 실패하였습니다.
                                    }

                                    btnSearch3_Click(null, null);
                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboHistOp.GetBindValue() == null)
                {
                    Util.Alert("FM_ME_0107"); //공정을 선택해주세요.
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("SHFT_ID", typeof(string));
                dtRqst.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("DFCT_CODE", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("S71", typeof(string));
                dtRqst.Columns.Add("REMARKS_CNTT", typeof(string));
                dtRqst.Columns.Add("UPDUSER", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["FROM_DATE"] = dtpHistFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpHistFromTime.DateTime.Value.ToString(" HH:mm:00");
                dr["TO_DATE"] = dtpHistToDate.SelectedDateTime.ToString("yyyy-MM-dd") + dtpHistToTime.DateTime.Value.ToString(" HH:mm:00");

                //dr["SHFT_ID"] = cboHistShift.GetBindValue();
                dr["DFCT_GR_TYPE_CODE"] = cboHistOp.GetBindValue();
                //dr["DFCT_CODE"] = cboHistDefect.GetBindValue();
                dr["SUBLOTID"] = txtHistCellId.GetBindValue();
                if (cboHistEqp.Visibility == Visibility.Visible)
                {
                    dr["S71"] = cboHistLane.GetBindValue();
                    dr["EQPTID"] = cboHistEqp.GetBindValue();
                }
                dr["MDLLOT_ID"] = cboHistModel.GetBindValue();
                dr["PROD_LOTID"] = txtHistLotId.GetBindValue();
                //dr["REMARKS_CNTT"] = txtHistHoldDesc.GetBindValue();
                dr["UPDUSER"] = txtHistUser.GetBindValue();

                dtRqst.Rows.Add(dr);

                //Todo: 같은 Form에 두개이상의 메뉴를 사용할 경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TC_CELL_LOSS_DEL", "RQSTDT", "RSLTDT", dtRqst, menuid: Tag.ToString());

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SUBLOT_TRANSFER_HOLD_LOT_RESTORE_HIST", "RQSTDT", "RSLTDT", dtRqst, gsEqpKind);

                Util.GridSetData(dgHistList, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoReg_Checked(object sender, RoutedEventArgs e)
        {
            if (rdoReg.IsChecked == true)
            {
                dtpFromTime.IsEnabled = true;
                dtpToTime.IsEnabled = true;
            }
            else
            {
                dtpFromTime.IsEnabled = false;
                dtpToTime.IsEnabled = false;
            }
        }

        private void rdoGoodDefect_Checked(object sender, RoutedEventArgs e)
        {
            if (rdoGood.IsChecked == true)
            {
                bGood = true;
                bDefect = false;

                if (rdoDefect != null) rdoDefect.IsChecked = false;
            }
            else
            {
                bGood = false;
                bDefect = true;

                if (rdoGood != null) rdoGood.IsChecked = false;
            }
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            LoadExcel(dgInputList);
        }


        private void btnExcel2_Click(object sender, RoutedEventArgs e)
        {
            LoadExcel(dgRecover);
        }


        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgInputList, true);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgInputList, false);
        }

        private void dgInputList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("SUBLOTID"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Black);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Normal;
                    }
                }
            }));
        }

        private void dgInputList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("DENY_DESC"))
                {
                    if (dataGrid[e.Cell.Row.Index, 0].Presenter != null)
                    {
                        if (!string.IsNullOrEmpty(Util.NVC(e.Cell.Value)))
                        {
                            CheckBox cb = dataGrid[e.Cell.Row.Index, 0].Presenter.Content as CheckBox;
                            if (cb != null) cb.Visibility = Visibility.Hidden;
                        }
                        else
                        {

                            CheckBox cb = dataGrid[e.Cell.Row.Index, 0].Presenter.Content as CheckBox;
                            cb.Visibility = Visibility.Visible;
                        }
                    }
                }

                //if (e.Cell.Row.Index == 0 && e.Cell.Column.Name.Equals("SUBLOTID"))
                //{
                //    C1.WPF.DataGrid.DataGridColumnHeaderPresenter dgcp = (C1.WPF.DataGrid.DataGridColumnHeaderPresenter)e.Cell.Presenter.MergedRange.TopLeftCell.Presenter.Content;
                //    dgcp.Foreground = Brushes.Red;
                //}
            }));
        }

        private void LoadExcel(C1.WPF.DataGrid.C1DataGrid dataGrid)
        {
            DataTable dtInfo = DataTableConverter.Convert(dataGrid.ItemsSource);

            dtInfo.Clear();

            OpenFileDialog fd = new OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";


            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    C1XLBook book = new C1XLBook();
                    book.Load(stream, FileFormat.OpenXml);
                    XLSheet sheet = book.Sheets[0];
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("SUBLOTID", typeof(string));
                    for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        // CELL ID;
                        if (sheet.GetCell(rowInx, 0) == null)
                            return;

                        string CELL_ID = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                        DataRow dataRow = dataTable.NewRow();
                        dataRow["SUBLOTID"] = CELL_ID;
                        dataTable.Rows.Add(dataRow);
                    }

                    if (dataTable.Rows.Count > 0)
                        dataTable = dataTable.DefaultView.ToTable(true);

                    Util.GridSetData(dataGrid, dataTable, FrameOperation);
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;
            dgRecover.RemoveRow(idx);
        }

        private void dgRecover_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            if (e.Column.Name != null && e.Column.Name.Equals("SUBLOTID"))
            {
                e.Column.HeaderPresenter.Foreground = Brushes.Red;
            }
            else
            {
                e.Column.HeaderPresenter.Foreground = Brushes.Black;
            }
        }

        private void txtInputDummyTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtInputDummyTrayID.Text.Trim() == string.Empty)
                    return;

                Receive_ScanMsg(txtInputDummyTrayID.Text.ToUpper().Trim());

            }
        }

        private void Receive_ScanMsg(string sScan)
        {
            string sResultMsg = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(sScan) || sScan.Length != 10)
                {
                    //잘못된 ID입니다.
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0205"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        txtInputDummyTrayID.SelectAll();
                        txtInputDummyTrayID.Focus();
                    });
                    return;
                }

                txtDummyTrayID.Text = sScan.Trim();

                // Tray 정보 조회
                GetTrayInfo();
                // Tray 타입 조회
                GetTrayType(txtDummyTrayID.Text);

                string sSublot = dgCell.GetValue(0, "SUBLOTID").ToString();

                // 재작업 Route List Setting
                SetIsReWorkRoute(sSublot, txtDummyTrayID.Text, txtDummyLotID.Text, _TrayType);

                txtInputDummyTrayID.Clear();
                txtInputDummyTrayID.Focus();

                return;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetTrayInfo()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("CSTID", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["CSTID"] = txtDummyTrayID.Text;
                dr["LANGID"] = LoginInfo.LANGID;

                INDATA.Rows.Add(dr);
                inDataSet.Tables.Add(INDATA);

                ShowLoadingIndicator();

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_TRAY_INFO_FOR_HOLD", "INDATA", "OUT_SUBLOT,OUT_CST", inDataSet);

                // Cell 정보 Grid Setting
                Util.GridSetData(dgCell, dsRslt.Tables["OUT_SUBLOT"], FrameOperation, true);

                // 현재 라인 Setting
                txtEqsgID.Text = dsRslt.Tables["OUT_CST"].Rows[0]["ROUT_EQSGID"].ToString();
                // 대상 라인 Combo에 현재 라인 셋팅
                if (chkLineAuto.IsChecked == true)
                {
                    cboDummyLineID.SelectedValue = dsRslt.Tables["OUT_CST"].Rows[0]["ROUT_EQSGID"].ToString().Trim();
                }
                // PKG Lot Setting
                txtDummyLotID.Text = dsRslt.Tables["OUT_CST"].Rows[0]["PROD_LOTID"].ToString();
                // Model Setting
                txtDummyModel.Text = dsRslt.Tables["OUT_CST"].Rows[0]["MDLLOT_ID"].ToString();
                // Cell 수량 Setting
                txtDummyCellCnt.Text = Convert.ToString(Convert.ToInt32((decimal)dsRslt.Tables["OUT_CST"].Rows[0]["WIPQTY"]));
                // 제품 Setting
                txtDummyProdCD.Text = dsRslt.Tables["OUT_CST"].Rows[0]["PRODID"].ToString();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void AllClear()
        {
            Util.gridClear(dgCell);
            lstRoute.ItemsSource = null;

            txtDummyTrayID.Text = string.Empty;
            txtDummyCellCnt.Text = string.Empty;
            txtDummyModel.Text = string.Empty;
            txtDummyLotID.Text = string.Empty;
            txtDummyProdCD.Text = string.Empty;
            txtEqsgID.Text = string.Empty;

            txtSpecial.Text = string.Empty;
            txtUserName.Text = string.Empty;
            txtUserID.Text = string.Empty;

            _TrayType = string.Empty;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            AllClear();

            txtInputDummyTrayID.Focus();
            txtInputDummyTrayID.SelectAll();
        }

        private void btnCreateDummy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(lstRoute.SelectedValue.ToString()))
                {
                    // 공정경로를 선택해주세요.
                    Util.MessageInfo("FM_ME_0106");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtDummyLotID.Text))
                {
                    // Lot ID를 입력해주세요.
                    Util.MessageInfo("FM_ME_0049");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtDummyTrayID.Text))
                {
                    // Tray ID를 입력해주세요..
                    Util.MessageInfo("FM_ME_0070");
                    return;
                }

                if (cboDummyLineID.SelectedValue.ToString() == "" || cboDummyLineID.SelectedValue.ToString() == "SELECT")
                {
                    // 라인을 선택하세요.
                    Util.MessageInfo("SFU1223");
                    return;
                }

                if (!Util.NVC(cboSpecial.SelectedValue).Equals("N"))
                {
                    if (string.IsNullOrEmpty(txtSpecial.Text))
                    {
                        //관리내역을 입력해주세요.
                        Util.MessageValidation("FM_ME_0113");  
                        return;
                    }
                }

                DataSet ds = new DataSet();

                DataTable dt = ds.Tables.Add("INDATA");

                dt.Columns.Add("SRCTYPE", typeof(string));
                dt.Columns.Add("IFMODE", typeof(string));
                dt.Columns.Add("USERID", typeof(string));
                dt.Columns.Add("LANGID", typeof(string)); //추가
                dt.Columns.Add("PROD_LOTID", typeof(string));
                dt.Columns.Add("ROUTID", typeof(string));
                dt.Columns.Add("CSTID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("SPCL_FLAG", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));
                dt.Columns.Add("SPCL_DESC", typeof(string));
                dt.Columns.Add("JIG_REWORK_YN", typeof(string));
                dt.Columns.Add("REQ_USERID", typeof(string));
                dt.Columns.Add("STORAGE_YN", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                //특별 예상 해제일 추가 2021.06.30 PSM
                dt.Columns.Add("FORM_SPCL_REL_SCHD_DTTM", typeof(string));

                DataRow dr = dt.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["IFMODE"] = "OFF";
                dr["USERID"] = LoginInfo.USERID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROD_LOTID"] = txtDummyLotID.Text;
                dr["ROUTID"] = lstRoute.SelectedValue.ToString();
                dr["CSTID"] = txtDummyTrayID.Text;  //Tray ID를 입력해주세요.
                dr["EQSGID"] = cboDummyLineID.SelectedValue.ToString();
                dr["SPCL_FLAG"] = Util.GetCondition(cboSpecial);
                dr["PRODID"] = Util.GetCondition(txtDummyProdCD);
                dr["SPCL_DESC"] = txtSpecial.Text;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                if ((bool)chkReleaseDate.IsChecked)   //특별 예상 해제일 추가 2021.06.30 PSM
                {
                    dr["FORM_SPCL_REL_SCHD_DTTM"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                }

                dt.Rows.Add(dr);

                DataTable dtCell = ds.Tables.Add("IN_CELL");
                dtCell.Columns.Add("SUBLOTID", typeof(string));
                dtCell.Columns.Add("CSTSLOT", typeof(string));

                DataRow drCell = null;
                for (int i = 0; i < dgCell.Rows.Count; i++)
                {
                    drCell = dtCell.NewRow();
                    drCell["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "SUBLOTID"));
                    drCell["CSTSLOT"] = (i + 1).ToString();

                    dtCell.Rows.Add(drCell);
                }

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService_Multi("BR_SET_CHANGE_ROUTE_FOR_HOLD_TRAY", "INDATA,IN_CELL", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult.Tables["OUTDATA"].Rows[0]["RESULT"].ToString().Equals("0"))
                        {
                            //생성완료하였습니다.
                            Util.MessageInfo("FM_ME_0160");
                            btnClear_Click(null, null);
                            btnSearch_Click(null, null);
                        }
                        else
                        {
                            //생성실패하였습니다. (Result Code : {0})
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0012", bizResult.Tables["OUTDATA"].Rows[0]["RESULT"].ToString()), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                }
                            });
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, ds);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool SetIsReWorkRoute(string sCellId, string sTrayId, string sLotId, string sTrayType)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = ds.Tables.Add("INDATA");

                dt.TableName = "INDATA";
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("SUBLOTID", typeof(string));
                dt.Columns.Add("CST_TYPE_CODE", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = sCellId;
                dr["CST_TYPE_CODE"] = sTrayType;
                dr["LOTID"] = sLotId;
                dr["EQSGID"] = cboDummyLineID.SelectedValue.ToString();
                dt.Rows.Add(dr);

                ShowLoadingIndicator();

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_REWORK_ROUTE", "INDATA", "OUTDATA", ds, null);

                //재작업 기준정보가 없다면 알람 후 종료
                if (dsResult.Tables["OUTDATA"].Rows.Count == 0)
                {
                    //재작업 라우트 기준정보가 존재하지 않습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0209"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            AllClear();
                        }
                    });
                    return false;
                }

                lstRoute.ItemsSource = dsResult.Tables["OUTDATA"].AsDataView();
                lstRoute.SelectedValuePath = "ROUTID";
                lstRoute.DisplayMemberPath = "CMCDNAME";

                if (lstRoute.Items.Count > 0)
                    lstRoute.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

            return true;
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnSearchUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName;

            userName = txtUserName.Text;

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += new EventHandler(wndUser_Closed);
            grdMain.Children.Add(wndPerson);
            wndPerson.BringToFront();
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = wndPerson.USERNAME;
                txtUserID.Text = wndPerson.USERID;
            }
        }

        private void cboSpecial_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.NVC(e.NewValue).Equals("N"))
            {
                txtSpecial.IsEnabled = false;
            }
            else
            {
                txtSpecial.IsEnabled = true;
            }
        }

        private void chkLineAuto_Checked(object sender, RoutedEventArgs e)
        {
            if (cboDummyLineID == null) return;
            cboDummyLineID.IsEnabled = true;
        }

        private void chkLineAuto_Unchecked(object sender, RoutedEventArgs e)
        {
            if (cboDummyLineID == null) return;
            cboDummyLineID.IsEnabled = false;
        }

        private void chkReleaseDate_Checked(object sender, RoutedEventArgs e)
        {
            dtpFromDate01.IsEnabled = true;
            dtpFromTime01.IsEnabled = true;
        }

        private void chkReleaseDate_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpFromDate01.IsEnabled = false;
            dtpFromTime01.IsEnabled = false;
        }

        private void dgCell_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgCell.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void GetTrayType(string sTray)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CSTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CSTID"] = sTray;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_F", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                {
                    _TrayType = SearchResult.Rows[0]["TRAY_TYPE_CODE"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool ChkErpClosingFlag(DateTime RegDttm)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("REG_DTTM", typeof(DateTime));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["REG_DTTM"] = RegDttm;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_CHK_ERP_CLOSING_FLAG", "INDATA", null, dtRqst);

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
