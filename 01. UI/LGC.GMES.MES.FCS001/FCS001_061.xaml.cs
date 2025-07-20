/*************************************************************************************************************
 Created Date : 2023.12.07
   Decription : Aging 예약 취소
--------------------------------------------------------------------------------------------------------------
 [Change History]
  2023.12.07  조영대 : Aging 예약 취소 화면 신규 생성 (Copy by FCS001_031)
  **************************************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_061 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string EQPT_GR_TYPE_CODE = string.Empty;
        private string NEXT_PROC = string.Empty;
        private Dictionary<string, string> DEFAULT_NEXT_PROC = null;

        Util _Util = new Util();

        public FCS001_061()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                //사용자 권한별로 버튼 숨기기
                List<Button> listAuth = new List<Button>();
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
                //여기까지 사용자 권한별로 버튼 숨기기

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
            cboLotId.Visibility = Visibility.Visible;
            cboLotId.IsEnabled = true;
            txtLotID.Visibility = Visibility.Collapsed;
            txtLotID.IsEnabled = false;
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            C1ComboBox[] cboAgingTypeChild = { cboLotId };
            string[] sFilterAgingType = { "EQPT_GR_TYPE_CODE", "3,7" };
            _combo.SetCombo(cboAgingType, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilterAgingType, cbChild: cboAgingTypeChild);

            string[] sFilterToOp = new string[] { "EQPT_GR_TYPE_CODE" };
            C1ComboBox[] cboNextOpChild = { cboLotId, cboRoute };
            _combo.SetCombo(cboNextOp, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilterToOp, cbChild: cboNextOpChild);

            string[] sFilter2 = { "COMBO_AGING_ISS_PRIORITY" }; // 기준정보 추가요청중.
            _combo.SetCombo(cboPriority, CommonCombo_Form.ComboStatus.NONE, sCase: "CMN", sFilter: sFilter2);

            C1ComboBox[] cboLineChild = { cboModel, cboLotId };
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.SELECT, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };

            C1ComboBox[] cboModelChild = { cboRoute, cboLotId };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);


            C1ComboBox[] cboRouteParent = { cboLine, cboModel, cboNextOp };
            C1ComboBox[] cboRouteChild = { cboOper, cboLotId };
            _combo.SetCombo(cboRoute, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_NEXTOP", cbParent: cboRouteParent, cbChild: cboRouteChild);


            // Lot 유형
            //2023.01.06   LotType  default  양산  
            string[] sFilterLotType = { "P" };
            _combo.SetCombo(cboLotType, CommonCombo_Form.ComboStatus.SELECT, sCase: "LOTTYPE", sFilter: sFilterLotType); //2022.12.21 Lot 유형 검색조건 추가



            C1ComboBox[] cboLotParent = { cboLine, cboModel, cboRoute, cboSpecial, cboAgingType, cboNextOp };
            //_combo.SetCombo(cboLotId, CommonCombo_Form.ComboStatus.ALL, sCase: "LOTID", cbParent: cboLotParent); //2021.04.04  KDH : PKG Lot List 조회 수정
            _combo.SetCombo(cboLotId, CommonCombo_Form.ComboStatus.ALL, sCase: "LOT", cbParent: cboLotParent); //2021.04.04  KDH : PKG Lot List 조회 수정


            C1ComboBox[] cboOperParent = { cboRoute };
            string[] sFilterOper = { "3,7" };
            _combo.SetCombo(cboOper, CommonCombo_Form.ComboStatus.ALL, sCase: "ROUTE_OP_MAX_END_TIME", cbParent: cboOperParent, sFilter: sFilterOper);

            string[] sFilter = { "COMBO_FORM_SPCL_FLAG" };

            C1ComboBox[] cboSpecialChild = { cboLotId };
            _combo.SetCombo(cboSpecial, CommonCombo_Form.ComboStatus.ALL, sCase: "CMN", sFilter: sFilter, cbChild: cboSpecialChild);


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

            // 차기 공정 기본 설정
            if (DEFAULT_NEXT_PROC != null && DEFAULT_NEXT_PROC.ContainsKey(EQPT_GR_TYPE_CODE))
            {
                cboNextOp.SelectedValue = DEFAULT_NEXT_PROC[EQPT_GR_TYPE_CODE];
            }
        }

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.ClearValidation();
            if (!dgAgingReserv.IsCheckedRow("CHK"))
            {
                Util.Alert("SFU1645"); //선택된 작업대상이 없습니다.
                return;
            }

            string sMSG = "FM_ME_0313"; // 예약취소하시겠습니까?

            Util.MessageConfirm(sMSG, (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                            DataTable dtRqst = new DataTable();
                            dtRqst.TableName = "INDATA";
                            dtRqst.Columns.Add("LOTID", typeof(string));
                            DataTable dtAR = DataTableConverter.Convert(dgAgingReserv.ItemsSource);

                            string strLOTID = "";        //input data
                            int sCnt = 0;
                           
                            foreach (DataRow drar in dtAR.Rows)
                            {
                                if (Util.NVC(drar["CHK"]).Equals("True"))
                                {
                                    strLOTID += Util.NVC(drar["LOTID"]) + ",";
                                    sCnt++;
                                }
                            }

                            DataRow dr = dtRqst.NewRow();
                            dr["LOTID"] = strLOTID.TrimEnd(',');    //,마지막 제거
                            dtRqst.Rows.Add(dr);

                            if (dtRqst.Rows.Count == 0)
                            {
                                Util.MessageValidation("FM_ME_0165");  //선택된 데이터가 없습니다.
                                return;
                            }
                                          
                            new ClientProxy().ExecuteService("BR_SET_AGING_RESERVATION_CANCEL", "INDATA", "OUTDATA", dtRqst, (shopResult, shopException) =>
                            {
                                if (shopException != null)
                                {
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(shopException), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                Util.MessageValidation(string.Format("{0}개의 Tray를 예약취소하였습니다.", sCnt));
                                GetList();
                                return;
                            }
                        );
                   }
 
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });

        }

        private void cboPriority_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgAgingReserv);
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
                        this.FrameOperation.OpenMenu("SFU010710010", true, parameters); //Tray 정보조회
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
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgAgingReserv.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!cell.Column.Name.Equals("CHK")) return;

                if (cell.Row.Index < 2) return;

                dgAgingReserv.ClearValidation();

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
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name.ToString().Equals("CSTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                }
            }));

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

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            dgAgingReserv.ClearValidation();

            Util.DataGridCheckAllChecked(dgAgingReserv);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgAgingReserv);
        }

        private object xProgress_WorkProcess(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            /* 10개씩 분리 처리
             * LOT ID 가 중복해서 들어가면 안되어 체크 후 처리.
             * 같은 UNIT 에서 분리되어 안들어가게 처리.
             */
            try
            {
                object[] arguments = e.Arguments as object[];

                List<string> processTrayId = new List<string>();

                DataTable dtRqstAll = arguments[0] as DataTable;

                DataTable dtRqst = dtRqstAll.Clone();

                int reservCnt = 0;
                int addCnt = 0;
                foreach (DataRow dataRow in dtRqstAll.Rows)
                {
                    if (processTrayId.Contains(Util.NVC(dataRow["LOTID"]))) continue;

                    addCnt++;
                    // MES 2.0 ItemArray 위치 오류 Patch
                    //dtRqst.Rows.Add(dataRow.ItemArray);
                    dtRqst.AddDataRow(dataRow);
                    processTrayId.Add(Util.NVC(dataRow["LOTID"]));

                    // 해당 LOTID 의 같은 UNIT 가 있으면 같이 포함.
                    List<DataRow> chkRows = dtRqstAll.AsEnumerable()
                        .Where(w => w.Field<string>("UNITID").Equals(Util.NVC(dataRow["UNITID"])) && !w.Field<string>("LOTID").Equals(Util.NVC(dataRow["LOTID"])))
                        .ToList();
                    if (chkRows != null && chkRows.Count > 0)
                    {
                        foreach (DataRow addRow in chkRows)
                        {
                            if (processTrayId.Contains(Util.NVC(addRow["LOTID"]))) continue;

                            addCnt++;
                            // MES 2.0 ItemArray 위치 오류 Patch
                            //dtRqst.Rows.Add(addRow.ItemArray);
                            dtRqst.AddDataRow(addRow);
                            processTrayId.Add(Util.NVC(addRow["LOTID"]));
                        }
                    }

                    if (dtRqst.Rows.Count >= 10 || addCnt >= dtRqstAll.Rows.Count)
                    {
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_AGING_UNLOAD_RESERVATION", "INDATA", "OUTDATA", dtRqst, menuid: FrameOperation.MENUID.ToString());
                        if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                        {
                            reservCnt += Util.NVC_Int(dtRslt.Rows[0]["RESER_CNT"]);
                        }
                        else
                        {
                            return Util.NVC(dtRslt.Rows[0]["MSGNAME"]);
                        }

                        e.Worker.ReportProgress(Convert.ToInt16((double)addCnt / (double)dtRqstAll.Rows.Count * 100));

                        dtRqst.Rows.Clear();
                    }
                }
                return reservCnt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return null;
        }

        private void xProgress_WorkProcessChanged(object sender, int percent, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                xProgress.Percent = percent;
                xProgress.ProgressText = "Working...";
                xProgress.InvalidateVisual();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void xProgress_WorkProcessCompleted(object sender, CMM001.Controls.UcProgress.WorkProcessEventArgs e)
        {
            try
            {
                xProgress.Visibility = Visibility.Collapsed;

                object[] arguments = e.Arguments as object[];
                string sMsg = arguments[1] as string;

                if (e.Result != null && e.Result is int)
                {
                    int reservCnt = Util.NVC_Int(e.Result);

                    Util.AlertInfo(sMsg, new string[] { reservCnt.ToString() });
                }
                else if (e.Result != null && e.Result is string)
                {
                    string msg = MessageDic.Instance.GetMessage("FM_ME_0202") + "\r\n\r\n" + Util.NVC(e.Result);
                    Util.AlertInfo(msg);  //작업중 오류가 발생하였습니다.
                }
                else
                {
                    string msg = MessageDic.Instance.GetMessage("FM_ME_0202");
                    Util.AlertInfo(msg);  //작업중 오류가 발생하였습니다.
                }

                GetList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void dgAgingReserv_ExecuteDataModify(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            DataTable dtResult = e.ResultData as DataTable;
            if (dtResult.Columns.Contains("CHK")) dtResult.Columns.Remove("CHK");                

            dtResult.Columns.Add("CHK", typeof(bool));
            dtResult.Select().ToList<DataRow>().ForEach(r => r["CHK"] = false);
        }

        private void dgAgingReserv_ExecuteDataCompleted(object sender, CMM001.Controls.UcBaseDataGrid.ExecuteDataEventArgs e)
        {

        }
        #endregion

        #region Method

        private void GetList()
        {
            try
            {
                this.ClearValidation();

                if (cboLine.GetBindValue() == null)
                {
                    cboLine.SetValidation(MessageDic.Instance.GetMessage("FM_ME_0044"), true);
                    return;
                }

                if (cboLotType.GetBindValue() == null)
                {
                    cboLotType.SetValidation(MessageDic.Instance.GetMessage("FM_ME_0050"), true);
                    return;
                }

                Util.gridClear(dgAgingReserv);

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

                dtRqst.Columns.Add("LOTTYPE", typeof(string));  //2022.12.21  LotType  조회조건 추가

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboOper, bAllNull: true);

                dr["LOTTYPE"] = Util.GetCondition(cboLotType, sMsg: "FM_ME_0050");  //2022.12.21  LotType  조회조건 추가


                dr["PROD_LOTID"] = Util.GetCondition(cboLotId, bAllNull: true);
                dr["TO_PROCID"] = string.IsNullOrEmpty(NEXT_PROC) ? "5,D" : NEXT_PROC;


                dr["SPCL_FLAG"] = Util.GetCondition(cboSpecial, bAllNull: true);
                dr["AGING_ISS_PRIORITY_NO"] = Util.GetCondition(cboPriority);
                dr["PROC_GR_CODE"] = Util.GetCondition(cboAgingType, sMsg: "FM_ME_0336"); //Aging 유형을 선택해주세요.
                dr["TO_PROC_FIX"] = Util.GetCondition(cboNextOp, sMsg: "FM_ME_0338"); //차기공정을 선택해주세요.
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = Util.GetCondition(cboSCLine, bAllNull: true);

                dr["WIPSTAT"] = "PROC";
                dr["ISS_RSV_FLAG"] = "N";
                dr["ABNORM_FLAG"] = "N";

                dtRqst.Rows.Add(dr);

                dgAgingReserv.ExecuteService("BR_GET_AGING_CAN_UNLOAD_TRAY_FOR_CANCEL", "RQSTDT", "RSLTDT", dtRqst);

            }
            catch (Exception e)
            {
                Util.MessageException(e);
            }
        }

        #endregion

        private void cboAgingType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            EQPT_GR_TYPE_CODE = e.NewValue.ToString();
            string[] sFilter = { EQPT_GR_TYPE_CODE, null };
            _combo.SetCombo(cboSCLine, CommonCombo_Form.ComboStatus.ALL, sCase: "SCLINE", sFilter: sFilter);
            if (cboSCLine.Items.Count > 0)
            {
                cboSCLine.SelectedIndex = 0;
            }

            // 차기공정 기본 설정(AGING_NEXT_PROC Attribute1, Attribute2 매칭)
            if (DEFAULT_NEXT_PROC != null && DEFAULT_NEXT_PROC.ContainsKey(EQPT_GR_TYPE_CODE))
            {
                cboNextOp.SelectedValue = DEFAULT_NEXT_PROC[EQPT_GR_TYPE_CODE];
            }
        }


        /// <summary>
        /// 동일 Rack  tray 체크 및 해제 이벤트 
        /// </summary>
        /// <param name="EQPTID"></param>
        private void sameEqptid_CheckEvent(object sender, RoutedEventArgs e)
        {
            CheckBox rb = sender as CheckBox;

            int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;   //클릭한 row index 가져옴
            string myEqptid  = Util.NVC(DataTableConverter.GetValue(dgAgingReserv.Rows[idx].DataItem, "EQPTID"));   //클릭한 녀석 EQPTID
            string chkValue = Util.NVC(DataTableConverter.GetValue(dgAgingReserv.Rows[idx].DataItem, "CHK")).ToString().Equals("True") ? "true" : "false";   //현제 checked 여부

            for (int i = 1; i <= dgAgingReserv.GetRowCount(); i++)    // 동일 tray 찾은 후  체크 또는 해제
            {          
                if ( Util.NVC(DataTableConverter.GetValue(dgAgingReserv.Rows[i+1].DataItem, "EQPTID")).Equals(myEqptid) ){
                    DataTableConverter.SetValue(dgAgingReserv.Rows[i+1].DataItem, "CHK", chkValue);
                }
            }
        }

    
    }
}
