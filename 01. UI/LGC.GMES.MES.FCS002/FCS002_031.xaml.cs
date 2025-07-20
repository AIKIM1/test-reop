/*************************************************************************************
 Created Date : 2023-02-08
      Creator : KANG DONG HEE
   Decription : 상온/출하 Aging 예약
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.08  NAME : Initial Created

  **************************************************************************************/
#define SAMPLE_DEV

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;

namespace LGC.GMES.MES.FCS002
{
    public enum AgingReserv
    {
        NH_AGING_REV = 0,
        NP_AGING_REV_BOX_JIG = 1
    }

    public partial class FCS002_031 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private AgingReserv MenuKind;
        private DataTable tempTable = new DataTable();

        private string LANE_ID = string.Empty;
        private string EQPT_GR_TYPE_CODE = string.Empty;
        private string NEXT_PROC = string.Empty;

        public FCS002_031()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (FrameOperation.MENUID.ToString())
                {
                    case "SFU010710420": //상온/출하 Aging 예약
                        MenuKind = AgingReserv.NH_AGING_REV;
                        break;
                    case "SFU010710430": //상온/Pre Aging 예약(Box/Jig)
                        MenuKind = AgingReserv.NP_AGING_REV_BOX_JIG;
                        break;
                }

                //Control Setting
                InitControl();

                //Combo Setting
                InitCombo();

                this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitControl()
        {
            if (MenuKind == AgingReserv.NH_AGING_REV)
            {
                // 확인 후 수정
                btnSearch.ToolTip = ObjectDic.Instance.GetObjectName("UC_0004"); //※ 상온/출하 Aging 작업조건이\r\n최대(999999분)로 설정된 Tray와\r\n차기공정이 Degas/특성측정기인\r\nTray만 조회됩니다.

                cboLotId.Visibility = Visibility.Visible;
                cboLotId.IsEnabled = true;
                txtLotID.Visibility = Visibility.Collapsed;
                txtLotID.IsEnabled = false;
            }
            else if (MenuKind == AgingReserv.NP_AGING_REV_BOX_JIG)
            {
                cboLotId.Visibility = Visibility.Collapsed;
                cboLotId.IsEnabled = false;
                txtLotID.Visibility = Visibility.Visible;
                txtLotID.IsEnabled = true;
            }
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            // 사이트마다 차기 공정에 차이가 있어, 동별 공통코드로 변경할 수 있도록 수정.
            GetAgingNextProc(MenuKind.ToString());

            if (MenuKind == AgingReserv.NH_AGING_REV)
            {
                C1ComboBox[] cboAgingTypeChild = { cboLotId };
                string[] sFilterAgingType = { "EQPT_GR_TYPE_CODE", "3" };
                _combo.SetCombo(cboAgingType, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "FORM_CMN", sFilter: sFilterAgingType, cbChild: cboAgingTypeChild);

                string[] sFilterToOp = null;
                if (string.IsNullOrWhiteSpace(NEXT_PROC))
                {
                    sFilterToOp = new string[] { "EQPT_GR_TYPE_CODE", "5" };
                }
                else
                {
                    sFilterToOp = new string[] { "EQPT_GR_TYPE_CODE", NEXT_PROC };
                }
                C1ComboBox[] cboNextOpChild = { cboLotId };
                _combo.SetCombo(cboNextOp, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "FORM_CMN", sFilter: sFilterToOp, cbChild: cboNextOpChild);

                string[] sFilter2 = { "COMBO_AGING_ISS_PRIORITY" }; // 기준정보 추가요청중.
                _combo.SetCombo(cboPriority, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "CMN", sFilter: sFilter2);

                C1ComboBox[] cboLineChild = { cboModel, cboLotId };
                _combo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

                C1ComboBox[] cboModelChild = { cboRoute, cboLotId };
                C1ComboBox[] cboModelParent = { cboLine };
                _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

                C1ComboBox[] cboRouteParent = { cboLine, cboModel };
                C1ComboBox[] cboRouteChild = { cboOper, cboLotId };
                _combo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent, cbChild: cboRouteChild);

                C1ComboBox[] cboLotParent = { cboLine, cboModel, cboRoute, cboSpecial, cboAgingType, cboNextOp };
                _combo.SetCombo(cboLotId, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LOT", cbParent: cboLotParent); //2021.04.04  KDH : PKG Lot List 조회 수정

                C1ComboBox[] cboOperParent = { cboRoute };
                string[] sFilterOper = { "3" };
                _combo.SetCombo(cboOper, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE_OP_MAX_END_TIME", cbParent: cboOperParent, sFilter: sFilterOper);

                // 자동차
                //string[] sFilter = { "COMBO_FORM_SPCL_FLAG" };
                string[] sFilter = { "FORM_SPCL_FLAG_MCC" };
                C1ComboBox[] cboSpecialChild = { cboLotId };
                _combo.SetCombo(cboSpecial, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter, cbChild: cboSpecialChild);
            }
            else
            {
                string[] sFilterAgingType = { "EQPT_GR_TYPE_CODE", "3,9" };
                _combo.SetCombo(cboAgingType, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "FORM_CMN", sFilter: sFilterAgingType);

                string[] sFilterToOp = null;
                if (string.IsNullOrWhiteSpace(NEXT_PROC))
                {
                    sFilterToOp = new string[] { "EQPT_GR_TYPE_CODE", "1" };
                }
                else
                {
                    sFilterToOp = new string[] { "EQPT_GR_TYPE_CODE", NEXT_PROC };
                }
                _combo.SetCombo(cboNextOp, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "FORM_CMN", sFilter: sFilterToOp);

                string[] sFilter2 = { "COMBO_AGING_ISS_PRIORITY" }; // 기준정보 추가요청중.
                _combo.SetCombo(cboPriority, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "CMN", sFilter: sFilter2);

                C1ComboBox[] cboLineChild = { cboModel };
                _combo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.SELECT_ALL, sCase: "LINE", cbChild: cboLineChild);

                C1ComboBox[] cboModelChild = { cboRoute };
                C1ComboBox[] cboModelParent = { cboLine };
                _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

                C1ComboBox[] cboRouteParent = { cboLine, cboModel };
                C1ComboBox[] cboRouteChild = { cboOper };
                _combo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent, cbChild: cboRouteChild);

                C1ComboBox[] cboOperParent = { cboRoute };
                string[] sFilterOper = { "3,9" };
                _combo.SetCombo(cboOper, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE_OP_MAX_END_TIME", cbParent: cboOperParent, sFilter: sFilterOper);

                // 자동차
                //string[] sFilter = { "COMBO_FORM_SPCL_FLAG" };
                string[] sFilter = { "FORM_SPCL_FLAG_MCC" };
                _combo.SetCombo(cboSpecial, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter);
            }

            DataTable dtSearchCnt = new DataTable();
            dtSearchCnt.Columns.Add("CBO_CODE", typeof(string));
            dtSearchCnt.Columns.Add("CBO_NAME", typeof(string));
            for (int i = 1; i < 11; i++)
            {
                DataRow dr = dtSearchCnt.NewRow();
                dr["CBO_CODE"] = (i * 10).ToString();
                dr["CBO_NAME"] = (i * 10).ToString();
                dtSearchCnt.Rows.Add(dr);
            }
            DataRow drAll = dtSearchCnt.NewRow();
            drAll["CBO_CODE"] = "99999";
            drAll["CBO_NAME"] = "-ALL-";
            dtSearchCnt.Rows.Add(drAll);

            cboSearchChount.DisplayMemberPath = "CBO_NAME";
            cboSearchChount.SelectedValuePath = "CBO_CODE";
            cboSearchChount.ItemsSource = DataTableConverter.Convert(dtSearchCnt);
            cboSearchChount.SelectedIndex = 0; //2021.04.11 조회건수 첫번째 데이타 자동 선택

            // 2025.05.27 |csspoto| : 차기 공정 기본 설정
            if (NEXT_PROC != null)
            {
                cboNextOp.SelectedValue = NEXT_PROC;
            }
        }

        private void GetCommonCode()
        {
            try
            {
                LANE_ID = string.Empty;
                EQPT_GR_TYPE_CODE = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_AGING_TYPE_CODE";
                dr["COM_CODE"] = Util.GetCondition(cboAgingType);
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                {
                    EQPT_GR_TYPE_CODE = row["ATTR1"].ToString();
                    LANE_ID = row["ATTR2"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetAgingNextProc(string menuKind)
        {
            try
            {
                NEXT_PROC = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "AGING_NEXT_PROC";
                dr["COM_CODE"] = menuKind;
                dr["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                {
                    NEXT_PROC = row["ATTR1"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnReservation_Click(object sender, RoutedEventArgs e)
        {
            string sMSG = string.Empty;
            if ((sender as Button).Name.Equals("btnReservation"))
            {
                sMSG = "FM_ME_0312"; //예약하시겠습니까?

                if (!CheckReserveValidation()) return;
            }
            else
            {
                sMSG = "FM_ME_0313"; // 예약취소하시겠습니까?
            }

            //상태를 변경하시겠습니까?
            Util.MessageConfirm(sMSG, (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sCancel = string.Empty;
                        string sMsg = string.Empty;
                        if ((sender as Button).Name.Equals("btnReservation"))
                        {
                            sCancel = "N";
                            sMsg = "FM_ME_0014";  //{0}개의 Tray를 예약완료하였습니다.
                        }
                        else
                        {
                            sCancel = "Y";
                            sMsg = "FM_ME_0301";  //{0}개의 Tray를 예약취소하였습니다.
                        }

                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("SRCTYPE", typeof(string));
                        dtRqst.Columns.Add("IFMODE", typeof(string));
                        dtRqst.Columns.Add("LOTID", typeof(string));
                        dtRqst.Columns.Add("CANCEL_YN", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));

                        //2021.04.19 데이타 Check 상태를 인지 못하는 현상 수정 START
                        DataTable dtAR = DataTableConverter.Convert(dgAgingReserv.ItemsSource);

                        foreach (DataRow drar in dtAR.Rows)
                        {
                            if (Util.NVC(drar["CHK"]).Equals("True"))
                            {
                                DataRow dr = dtRqst.NewRow();
                                dr["SRCTYPE"] = "UI";
                                dr["IFMODE"] = "OFF";
                                dr["LOTID"] = Util.NVC(drar["LOTID"]);
                                dr["CANCEL_YN"] = sCancel;
                                dr["USERID"] = LoginInfo.USERID;
                                dtRqst.Rows.Add(dr);
                            }
                        }
                        //2021.04.19 데이타 Check 상태를 인지 못하는 현상 수정 END

                        if (dtRqst.Rows.Count == 0)
                        {
                            Util.MessageValidation("FM_ME_0165");  //선택된 데이터가 없습니다.
                            return;
                        }

                        //Todo: 같은 Form에 두개이상의 메뉴를 사용할 경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_AGING_UNLOAD_RESERVATION_MB", "INDATA", "OUTDATA", dtRqst, menuid: FrameOperation.MENUID.ToString());
                        if (dtRslt.Rows.Count > 0)
                        {
                            if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                Util.AlertInfo(sMsg, new string[] { dtRslt.Rows[0]["RESER_CNT"].ToString() });
                            }
                            else
                            {
                                Util.AlertInfo("FM_ME_0202");  //작업중 오류가 발생하였습니다.
                            }
                        }

                        GetList();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });

        }

        /*private void cboAgingType_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            //GetCommonCode();
            EQPT_GR_TYPE_CODE = cboAgingType.SelectedValue.ToString();
            string[] sFilter = { EQPT_GR_TYPE_CODE, null };
            _combo.SetCombo(cboSCLine, CommonCombo_Form.ComboStatus.ALL, sCase: "SCLINE", sFilter: sFilter);
            if (cboSCLine.Items.Count > 0)
            {
                cboSCLine.SelectedIndex = 0;
            }
        }
        */

        private void cboPriority_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.GetCondition(cboPriority).Equals("5"))
            {
                btnReservation.Visibility = Visibility.Visible;
                btnCancel.Visibility = Visibility.Collapsed;
                Util.gridClear(dgAgingReserv);
            }
            else
            {
                btnReservation.Visibility = Visibility.Collapsed;
                btnCancel.Visibility = Visibility.Visible;
                Util.gridClear(dgAgingReserv);
            }
        }

        private void dgAgingReserv_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgAgingReserv.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Column.Name.Equals("CSTID"))
                    {
                        //Tray 정보조회 화면 연계
                        object[] parameters = new object[6];
                        parameters[0] = Util.NVC(DataTableConverter.GetValue(dgAgingReserv.Rows[cell.Row.Index].DataItem, "CSTID"));
                        parameters[1] = Util.NVC(DataTableConverter.GetValue(dgAgingReserv.Rows[cell.Row.Index].DataItem, "LOTID"));
                        parameters[2] = string.Empty;
                        parameters[3] = string.Empty;
                        parameters[4] = string.Empty;
                        parameters[5] = string.Empty;
                        this.FrameOperation.OpenMenu("SFU010710300", true, parameters); //Tray 정보조회
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgAgingReserv_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //동일한 설비 체크하기
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgAgingReserv.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!cell.Column.Name.Equals("CHK"))
                {
                    return;
                }

                if (cell.Row.Index < 2)
                {
                    return;
                }

                bool bCheck = bool.Parse(Util.NVC(DataTableConverter.GetValue(dgAgingReserv.Rows[cell.Row.Index].DataItem, "CHK")));
                bCheck = bCheck.Equals(false) ? true : false;

                string sEqpID = Util.NVC(DataTableConverter.GetValue(dgAgingReserv.Rows[cell.Row.Index].DataItem, "MACHINE_EQPTID"));

                for (int i = 0; i < dgAgingReserv.Rows.Count; i++)
                {
                    if (i > 1)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgAgingReserv.Rows[i].DataItem, "MACHINE_EQPTID")).Equals(sEqpID))
                        {
                            if (dgAgingReserv.Rows[i].Presenter != null && dgAgingReserv.Rows[i].Presenter[dgAgingReserv.Columns["CHK"]].IsEnabled)
                            {
                                DataTableConverter.SetValue(dgAgingReserv.Rows[i].DataItem, "CHK", bCheck);
                            }
                        }
                    }
                }
            }
        }

        private void dgAgingReserv_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    DataRowView dr = (DataRowView)e.Cell.Row.DataItem;

                    string sEQPTID = Util.NVC(dr.Row["EQPTID"]);
                    string sLOT_HOLD_YN = Util.NVC(dr.Row["LOT_HOLD_YN"]);
                    string sSPECIAL_YN = Util.NVC(dr.Row["SPCL_FLAG"]);
                    string sSPECIAL_DESC = Util.NVC(dr.Row["SPECIAL_DESC"]);
                    string sSPECIAL_SHIP = Util.NVC(dr.Row["SPECIAL_SHIP"]);
                    string sOVER_MINTIME = Util.NVC(dr.Row["OVER_MINTIME"]);
                    string sAGING_OUT_PRIORITY = Util.GetCondition(cboPriority);

                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    /* 2015-03-18   정종덕   CSR ID:2563313] 자동차 전지 저전압 한계불량 초과 Lot 자동 Hold 요청의  件
                     * 변경 내용 :  LOT_HOLD  트레이 표시
                     */
                    if (sLOT_HOLD_YN.Equals("Y"))
                    {
                        if (e.Cell.Column.Name.ToString().Equals("CHK"))
                        {
                            if (sAGING_OUT_PRIORITY.Equals("8"))
                            {
                                e.Cell.Presenter.IsEnabled = true;
                                //2021-05-11
                                // DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK_FLAG", "Y");
                            }
                            else
                            {
                                e.Cell.Presenter.IsEnabled = false;
                                //2021-05-11
                                // DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK_FLAG", "N");
                            }
                            // e.Cell.Presenter.IsEnabled = sAGING_OUT_PRIORITY.Equals("8") ? true : false;
                        }
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightSalmon);
                    }

                    if (!string.IsNullOrEmpty(sSPECIAL_DESC))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }

                    //200114 KJE: 특별Tray 출하금지
                    if (sSPECIAL_SHIP.Equals("N"))
                    {
                        if (e.Cell.Column.Name.ToString().Equals("CHK"))
                        {
                            e.Cell.Presenter.IsEnabled = false;
                        }
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Plum);
                    }

                    DataRow[] drSelect = tempTable.Select("LOW_VOLT_JUDGE_YN = 'N' AND EQPTID='" + sEQPTID + "'");
                    if (drSelect.Length > 0)
                    {
                        if (e.Cell.Column.Name.ToString() == "CHK")
                        {
                            e.Cell.Presenter.IsEnabled = false;
                        }
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightSlateGray);
                    }

                    //200203 : KJE 최소시간 미충족 Tray 출하 금지
                    if (sOVER_MINTIME.Equals("N"))
                    {
                        if (e.Cell.Column.Name.ToString().Equals("CHK"))
                        {
                            e.Cell.Presenter.IsEnabled = false;
                        }
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }

                    if (e.Cell.Column.Name.ToString().Equals("CSTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //e.Cell.Presenter.FontWeight = FontWeights.Bold; //2021.04.01 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));

        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            //Util.DataGridCheckAllChecked(dgAgingReserv);
            //2021-05-11 Lot Hold, 특별관리 출하금지 등의 사유로 전체 선택 Validation 추가
            for (int i = dgAgingReserv.TopRows.Count; i < dgAgingReserv.Rows.Count - 1; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgAgingReserv.Rows[i].DataItem, "CHK_FLAG")).Equals("Y"))
                {
                    DataTableConverter.SetValue(dgAgingReserv.Rows[i].DataItem, "CHK", true);
                }
            }
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgAgingReserv);
        }
        #endregion

        #region Method
        private bool CheckReserveValidation()
        {
            if (dgAgingReserv.ItemsSource == null)
            {
                Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                return false;
            }

            DataTable dtGrid = ((DataView)dgAgingReserv.ItemsSource).ToTable();

            #region Hold 된 Tray와 일반 Tray 가 2단 적재되어 있을 경우 예약 Validation
            var cstList = (from t in dtGrid.AsEnumerable().Where(r => r["CHK"].Equals("True"))
                           select new
                           {
                               CSTID = t.Field<string>("CSTID"),
                               EQPTID = t.Field<string>("EQPTID")
                           }).Distinct().ToList();

            foreach (var cstInfo in cstList)
            {
                int rackCount = dtGrid.AsEnumerable().Where(r => r["EQPTID"].Equals(cstInfo.EQPTID)).Count();
                int rackHoldCount = dtGrid.AsEnumerable().Where(r => r["EQPTID"].Equals(cstInfo.EQPTID) &&
                                                                    r["LOT_HOLD_YN"].Equals("Y")).Count();
                // 다중적재 Tray 이면서 Hold 가 한개이면 (Hold된 Tray와 일반 Tray가 2단 적재되어 있을 경우)
                if (rackCount > 1 && rackHoldCount == 1)
                {
                    //Hold 된 Tray와 일반 Tray 가 2단 적재되어 있을 경우 예약할 수 없습니다.
                    Util.MessageValidation("FM_ME_0449");
                    return false;
                }
            }
            #endregion

            return true;
        }

        private void GetList()
        {
            try
            {
                this.ClearValidation();

                //if (cboLine.GetBindValue() == null)
                //{
                //    cboLine.SetValidation(MessageDic.Instance.GetMessage("FM_ME_0044"), true);
                //    return;
                //}

                //20231220 Vailation 추가
                if (cboPriority.SelectedValue.ToString() == "SELECT")
                {
                    Util.MessageValidation("SFU4149"); //구분을 선택하세요
                    return;
                }

                Util.gridClear(dgAgingReserv);
                tempTable = null;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));

                dtRqst.Columns.Add("SPCL_FLAG", typeof(string));
                dtRqst.Columns.Add("AGING_ISS_PRIORITY_NO", typeof(string));
                dtRqst.Columns.Add("TO_PROCID", typeof(string));
                dtRqst.Columns.Add("PROC_GR_CODE", typeof(string));
                dtRqst.Columns.Add("TO_PROC_FIX", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));

                dtRqst.Columns.Add("WIPSTAT", typeof(string));
                dtRqst.Columns.Add("ISS_RSV_FLAG", typeof(string));
                dtRqst.Columns.Add("ABNORM_FLAG", typeof(string));

                dtRqst.Columns.Add("FROM_AGING_ISS_SCHD_DTTM", typeof(string));
                dtRqst.Columns.Add("TO_AGING_ISS_SCHD_DTTM", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true); //Line을 선택해주세요.
               // dr["EQSGID"] = Util.GetCondition(cboLine, sMsg: "FM_ME_0044", bAllNull: true); //Line을 선택해주세요.
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboOper, bAllNull: true);

                if (MenuKind == AgingReserv.NH_AGING_REV)
                {
                    dr["PROD_LOTID"] = Util.GetCondition(cboLotId, bAllNull: true);
                    dr["TO_PROCID"] = string.IsNullOrEmpty(NEXT_PROC) ? "5,D" : NEXT_PROC;
                }
                else if (MenuKind == AgingReserv.NP_AGING_REV_BOX_JIG)
                {
                    dr["PROD_LOTID"] = Util.GetCondition(txtLotID, bAllNull: true);
                    dr["TO_PROCID"] = string.IsNullOrEmpty(NEXT_PROC) ? "J,1" : NEXT_PROC;
                }

                dr["SPCL_FLAG"] = Util.GetCondition(cboSpecial, bAllNull: true);
                dr["AGING_ISS_PRIORITY_NO"] = Util.GetCondition(cboPriority);
                dr["PROC_GR_CODE"] = Util.GetCondition(cboAgingType, sMsg: "FM_ME_0336"); //Aging 유형을 선택해주세요.
                dr["TO_PROC_FIX"] = Util.GetCondition(cboNextOp, bAllNull: true); //차기공정을 선택해주세요.
                //dr["TO_PROC_FIX"] = Util.GetCondition(cboNextOp, sMsg: "FM_ME_0338"); //차기공정을 선택해주세요.
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = Util.GetCondition(cboSCLine, bAllNull: true);

                dr["WIPSTAT"] = "PROC";
                dr["ISS_RSV_FLAG"] = "N";
                dr["ABNORM_FLAG"] = "N";

                if (chkOutDate.IsChecked.Equals(false))
                {
                    dr["FROM_AGING_ISS_SCHD_DTTM"] = dtpOutDate.SelectedDateTime.AddDays(-1).ToString("yyyy-MM-dd 00:00:00");
                    dr["TO_AGING_ISS_SCHD_DTTM"] = dtpOutDate.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                }

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                //Todo: 같은 Form에 두개이상의 메뉴를 사용할 경우 비즈실행시 menuid: Tag.ToString() 설정해야됨
                new ClientProxy().ExecuteService("BR_GET_AGING_CAN_UNLOAD_TRAY_MB", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            DataTable dtRsltCnt = result.Clone();

                            DataTable dtTable = GetNewData(result);

                            if (dtTable == null || dtTable.Rows.Count == 0)
                            {
                                return;
                            }

                            for (int i = 0; i < Convert.ToUInt32(Util.GetCondition(cboSearchChount)) && i < dtTable.Rows.Count; i++)
                            {
                                dtRsltCnt.ImportRow(dtTable.Rows[i]);
                            }

                            //2021-05-11 전체선택 체크 시 체크 불가능한 Row 선별을 위하여 Flag 컬럼 추가
                            dtRsltCnt.Columns.Add("CHK_FLAG");
                            for (int i = 0; i < dtRsltCnt.Rows.Count; i++)
                            {

                                //Row별 check Flag 확인

                                string sEQPTID = Util.NVC(dtRsltCnt.Rows[i]["EQPTID"]);
                                string sLOT_HOLD_YN = Util.NVC(dtRsltCnt.Rows[i]["LOT_HOLD_YN"]);
                                string sSPECIAL_YN = Util.NVC(dtRsltCnt.Rows[i]["SPCL_FLAG"]);
                                string sSPECIAL_SHIP = Util.NVC(dtRsltCnt.Rows[i]["SPECIAL_SHIP"]);
                                string sOVER_MINTIME = Util.NVC(dtRsltCnt.Rows[i]["OVER_MINTIME"]);
                                string sAGING_OUT_PRIORITY = Util.GetCondition(cboPriority);

                                dtRsltCnt.Rows[i]["CHK_FLAG"] = "Y"; // Y : 선택 가능, N : 선택 불가능

                                /* 2015-03-18   정종덕   CSR ID:2563313] 자동차 전지 저전압 한계불량 초과 Lot 자동 Hold 요청의  件
                                 * 변경 내용 :  LOT_HOLD  트레이 표시
                                 */
                                if (sLOT_HOLD_YN.Equals("Y"))
                                {
                                    if (sAGING_OUT_PRIORITY.Equals("8"))
                                    {
                                        dtRsltCnt.Rows[i]["CHK_FLAG"] = "Y";
                                    }
                                    else
                                    {
                                        dtRsltCnt.Rows[i]["CHK_FLAG"] = "N";
                                    }
                                }
                                //특별관리 트레이 출하금지 선택 시 출하금지
                                if (sSPECIAL_SHIP.Equals("N"))
                                {
                                    dtRsltCnt.Rows[i]["CHK_FLAG"] = "N";
                                }

                                //최소시간 미충족시 출하금지
                                if (sOVER_MINTIME.Equals("N"))
                                {
                                    dtRsltCnt.Rows[i]["CHK_FLAG"] = "N";
                                }

                                //저전압
                                if (dtRsltCnt.Rows[i]["LOW_VOLT_JUDGE_YN"].Equals("N"))
                                {
                                    dtRsltCnt.Rows[i]["CHK_FLAG"] = "N";
                                }
                            }
                            tempTable = dtRsltCnt.Copy();

                            Util.GridSetData(dgAgingReserv, dtRsltCnt, FrameOperation, true);
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
                });
            }
            catch (Exception e)
            {
                Util.MessageException(e);
            }
        }

        #endregion


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

        private void dgAgingReserv_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            if (e.Row.Index + 1 - dgAgingReserv.TopRows.Count > 0 && e.Row.Index != dgAgingReserv.Rows.Count - 1)
            {
                TextBlock tb = new TextBlock();
                tb.Text = (e.Row.Index + 1 - dgAgingReserv.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void dgAgingReserv_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                    e.Cell.Presenter.IsEnabled = true;
                }
            }
        }

        private void cboAgingType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            //GetCommonCode();
            EQPT_GR_TYPE_CODE = e.NewValue.ToString();
            string[] sFilter = { EQPT_GR_TYPE_CODE, null };
            _combo.SetCombo(cboSCLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "SCLINE", sFilter: sFilter);
            if (cboSCLine.Items.Count > 0)
            {
                cboSCLine.SelectedIndex = 0;
            }
        }

        private DataTable GetNewData(DataTable dt)
        {
            DataTable dtNewData = new DataTable();
            try
            {
                if (dt.Rows.Count > 0)
                {
                    dtNewData = dt.Copy();

                    if (chkLotHold.IsChecked.Equals(false))
                    {
                        if (dtNewData != null && dtNewData.Rows.Count > 0)
                        {
                            DataRow[] drSelect = dtNewData.Select("LOT_HOLD_YN <> 'Y'");
                            if (drSelect.Length > 0)
                            {
                                dtNewData = drSelect.CopyToDataTable();
                            }
                            else
                            {
                                dtNewData = null;
                            }
                        }
                    }

                    if (chkLowVolt.IsChecked.Equals(false))
                    {
                        if (dtNewData != null && dtNewData.Rows.Count > 0)
                        {
                            DataRow[] drSelect = dtNewData.Select("LOW_VOLT_JUDGE_YN <> 'N'");
                            if (drSelect.Length > 0)
                            {
                                dtNewData = drSelect.CopyToDataTable();
                            }
                            else
                            {
                                dtNewData = null;
                            }
                        }
                    }

                    if (chkSpecial.IsChecked.Equals(false))
                    {
                        if (dtNewData != null && dtNewData.Rows.Count > 0)
                        {
                            DataRow[] drSelect = dtNewData.Select("ISNULL(SPECIAL_DESC, '') = ''");
                            if (drSelect.Length > 0)
                            {
                                dtNewData = drSelect.CopyToDataTable();
                            }
                            else
                            {
                                dtNewData = null;
                            }
                        }
                    }

                    if (chkSpecialShipBan.IsChecked.Equals(false))
                    {
                        if (dtNewData != null && dtNewData.Rows.Count > 0)
                        {
                            DataRow[] drSelect = dtNewData.Select("SPECIAL_SHIP <> 'N'");
                            if (drSelect.Length > 0)
                            {
                                dtNewData = drSelect.CopyToDataTable();
                            }
                            else
                            {
                                dtNewData = null;
                            }
                        }
                    }

                    if (chkMinTime.IsChecked.Equals(false))
                    {
                        if (dtNewData != null && dtNewData.Rows.Count > 0)
                        {
                            DataRow[] drSelect = dtNewData.Select("OVER_MINTIME <> 'N'");
                            if (drSelect.Length > 0)
                            {
                                dtNewData = drSelect.CopyToDataTable();
                            }
                            else
                            {
                                dtNewData = null;
                            }
                        }
                    }
                }

                return dtNewData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
