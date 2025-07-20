using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_142.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_143 : UserControl, IWorkArea
    {
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public COM001_143()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCmb();
        }

        private void InitCmb()
        {
            CommonCombo _combo = new CommonCombo();
            // 동
            C1ComboBox[] cboInputAreaChild = { cboLine };
            String[] sFiltercboArea = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sFilter: sFiltercboArea, sCase: "AREA_AREATYPE", cbChild: cboInputAreaChild);


            //라인
            C1ComboBox[] cboInputEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboInputEquipmentSegmentChild = { cboPjt };
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.ALL, cbParent: cboInputEquipmentSegmentParent, cbChild: cboInputEquipmentSegmentChild, sCase: "EQUIPMENTSEGMENT");
            //cboLine.SelectedIndex = 0;
            C1ComboBox[] cboInputPJT_AbbrParent = { cboArea, cboLine};
            _combo.SetCombo(cboPjt, CommonCombo.ComboStatus.ALL, cbParent: cboInputPJT_AbbrParent, sCase: "cboPRJModelPack");

            Set_Tout_StatCode();
            Set_Tout_Lot_StatCode();
            Set_Tout_Overday();
        }

        /// <summary>
        /// 검색 조건 불출요청 상태 콤보 초기화
        /// </summary>
        private void Set_Tout_StatCode()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "RTLS_TOUT_STAT_CODE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboTot_Stat.DisplayMemberPath = "CBO_NAME";
                cboTot_Stat.SelectedValuePath = "CBO_CODE";
                cboTot_Stat.ItemsSource = DataTableConverter.Convert(dtResult);
                cboTot_Stat.CheckAll();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// 검색 조건 불출 요청랏 상태 콤보  초기화
        /// </summary>
        private void Set_Tout_Lot_StatCode()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "RTLS_TOUT_LOT_STAT_CODE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboTot_Lot_Stat.DisplayMemberPath = "CBO_NAME";
                cboTot_Lot_Stat.SelectedValuePath = "CBO_CODE";
                cboTot_Lot_Stat.ItemsSource = DataTableConverter.Convert(dtResult);
                cboTot_Lot_Stat.CheckAll();
                //cboTot_Lot_Stat.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        /// <summary>
        /// 검색 조건 경과일 콤보 초기화
        /// </summary>
        private void Set_Tout_Overday()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "RTLS_TOUT_OVER_DAY";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboOverDay.DisplayMemberPath = "CBO_NAME";
                cboOverDay.SelectedValuePath = "CBO_CODE";
                DataRow ro = dtResult.NewRow();
                ro["CBO_NAME"] = "-ALL-";
                ro["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(ro, 0);
                cboOverDay.ItemsSource = DataTableConverter.Convert(dtResult);
                cboOverDay.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// LOT 불출/ 회수 팝업 오픈
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInout_Click(object sender, RoutedEventArgs e)
        {
            COM001_143_InOut wndPopup = new COM001_143_InOut();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = "";
                Parameters[1] = LoginInfo.USERNAME;
                Parameters[2] = LoginInfo.USERID;
                Parameters[3] = string.Empty;  // 결제자
                Parameters[4] = string.Empty;  // 결제자 ID
                Parameters[5] = string.Empty;  // 요청 제목
                Parameters[6] = string.Empty;  // 요청 사유
                Parameters[7] = DateTime.Now;  // 요청 사유
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopupInout_Closed);
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));

            }
        }

        /// <summary>
        /// LOT 불출/ 회수 팝업 클로즈 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wndPopupInout_Closed(object sender, EventArgs e)
        {
            COM001_143_InOut window = sender as COM001_143_InOut;
            if (window.DialogResult == MessageBoxResult.OK)
            {
              
            }
            this.grdMain.Children.Remove(window);
        }

        /// <summary>
        /// 불출 요청 등록/수정 팝업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_143_Create wndPopup = new COM001_143_Create();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[8];
                    Parameters[0] = "";
                    Parameters[1] = LoginInfo.USERNAME;
                    Parameters[2] = LoginInfo.USERID;
                    Parameters[3] = string.Empty;  // 결제자
                    Parameters[4] = string.Empty;  // 결제자 ID
                    Parameters[5] = string.Empty;  // 요청 제목
                    Parameters[6] = string.Empty;  // 요청 사유
                    Parameters[7] = DateTime.Now;  // 요청 사유
                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    wndPopup.Closed += new EventHandler(wndPopupHot_Closed);
                    grdMain.Children.Add(wndPopup);
                    wndPopup.BringToFront();
                    //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.AlertError(ex);
            }

        }

        /// <summary>
        /// 불출 요청 등록/수정 팝업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iListIndex = fn_Get_Selected_Chk_Index();
                string currentStatcode = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[iListIndex].DataItem, "TOUT_APPR_STAT_CODE"));
                if (!currentStatcode.Equals("R"))
                {
                    //불출요청이 생성상태가 아닙니다. 
                    Util.Alert("RTLS0001");
                    return;

                }
                string toutID = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[iListIndex].DataItem, "TOUT_ID"));
                COM001_143_Create wndPopup = new COM001_143_Create();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[8];
                    Parameters[0] = toutID;
                    Parameters[1] = LoginInfo.USERNAME;
                    Parameters[2] = LoginInfo.USERID;
                    Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[iListIndex].DataItem, "APPR_USERNAME"));
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[iListIndex].DataItem, "TOUT_APPR_USER_ID"));
                    Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[iListIndex].DataItem, "TOUT_REQ_TITL"));
                    Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[iListIndex].DataItem, "TOUT_RSN_CNTT"));
                    Parameters[7] = Convert.ToDateTime(Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[iListIndex].DataItem, "CRRY_DATE")));
                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    wndPopup.Closed += new EventHandler(wndPopupHot_Closed);
                    grdMain.Children.Add(wndPopup);
                    wndPopup.BringToFront();

                }
            }
            catch (Exception ex)
            {

                Util.AlertError(ex);
            }

        }

        /// <summary>
        /// 불출 요청 등록/수정 팝업 클로즈
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wndPopupHot_Closed(object sender, EventArgs e)
        {
            COM001_143_Create window = sender as COM001_143_Create;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                Serarch_List_Return(window._reqNo);
            }
            this.grdMain.Children.Remove(window);
        }

        /// <summary>
        /// 불출 요청 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Serarch_List();
            }
            catch (Exception ex)
            {
                Util.AlertError(ex);
            }
        }
        /// <summary>
        /// 불출 요청 조회 펑션
        /// </summary>
        private void Serarch_List()
        {
            Util.gridClear(dgRequestList);
            Util.gridClear(dgRequest_Detail);
            txtRemark.Text = "";
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
         
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PJTNAME", typeof(string));
            RQSTDT.Columns.Add("LOTID", typeof(string));
            RQSTDT.Columns.Add("TOUT_ID", typeof(string));
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("TOUT_APPR_STAT_CODE", typeof(string));
            RQSTDT.Columns.Add("TOUT_STAT_CODE", typeof(string));
            RQSTDT.Columns.Add("OVER_DAY", typeof(string));
            RQSTDT.Columns.Add("FDATE", typeof(string));
            RQSTDT.Columns.Add("TDATE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["AREAID"] = cboArea.SelectedValue.ToString() == "" ? null : cboArea.SelectedValue.ToString();
            dr["EQSGID"] = cboLine.SelectedValue.ToString() == "" ? null : cboLine.SelectedValue.ToString();
            dr["PJTNAME"] = cboPjt.SelectedValue.ToString() == "" ? null : cboPjt.SelectedValue.ToString();
            dr["LOTID"] = txtLotId.Text == "" ? null : txtLotId.Text;
            dr["TOUT_ID"] = txtRequestId.Text == "" ? null : txtRequestId.Text;
            dr["LANGID"] = LoginInfo.LANGID;
            dr["TOUT_APPR_STAT_CODE"] = cboTot_Stat.SelectedItemsToString.ToString() == "" ? null : cboTot_Stat.SelectedItemsToString.ToString();
            dr["TOUT_STAT_CODE"] = cboTot_Lot_Stat.SelectedItemsToString.ToString() == "" ? null : cboTot_Lot_Stat.SelectedItemsToString.ToString();
            dr["OVER_DAY"] = cboOverDay.SelectedValue.ToString() == "" ? null : cboOverDay.SelectedValue.ToString();
            dr["FDATE"] = Util.GetCondition(dtpFDate);
            dr["TDATE"] = Util.GetCondition(dtpTDate);
            RQSTDT.Rows.Add(dr);

            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService("DA_RTLS_GET_TOUT", "RQSTDT", "RSLTDT", RQSTDT, (dt, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                if (dt.Rows.Count < 1)
                {
                    Util.Alert("SFU1905");
                    return;
                }

                Util.GridSetData(dgRequestList, dt, FrameOperation);
                Util.SetTextBlockText_DataGridRowCount(tbListCount, Util.NVC(dgRequestList.Rows.Count));
            });
        }

        private void Serarch_List_Return(string ToutID)
        {
            Util.gridClear(dgRequestList);
            Util.gridClear(dgRequest_Detail);
            txtRemark.Text = "";
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";

            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PJTNAME", typeof(string));
            RQSTDT.Columns.Add("LOTID", typeof(string));
            RQSTDT.Columns.Add("TOUT_ID", typeof(string));
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("TOUT_APPR_STAT_CODE", typeof(string));
            RQSTDT.Columns.Add("TOUT_STAT_CODE", typeof(string));
            RQSTDT.Columns.Add("OVER_DAY", typeof(string));
            RQSTDT.Columns.Add("FDATE", typeof(string));
            RQSTDT.Columns.Add("TDATE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            //dr["AREAID"] = cboArea.SelectedValue.ToString() == "" ? null : cboArea.SelectedValue.ToString();
            //dr["EQSGID"] = cboLine.SelectedValue.ToString() == "" ? null : cboLine.SelectedValue.ToString();
            //dr["PJTNAME"] = cboPjt.SelectedValue.ToString() == "" ? null : cboPjt.SelectedValue.ToString();
            //dr["LOTID"] = txtLotId.Text == "" ? null : txtLotId.Text;
            dr["TOUT_ID"] = ToutID;
            dr["LANGID"] = LoginInfo.LANGID;
            //dr["TOUT_APPR_STAT_CODE"] = cboTot_Stat.SelectedItemsToString.ToString() == "" ? null : cboTot_Stat.SelectedItemsToString.ToString();
            //dr["TOUT_STAT_CODE"] = cboTot_Lot_Stat.SelectedItemsToString.ToString() == "" ? null : cboTot_Lot_Stat.SelectedItemsToString.ToString();
            //dr["OVER_DAY"] = cboOverDay.SelectedValue.ToString() == "" ? null : cboOverDay.SelectedValue.ToString();
            //dr["FDATE"] = Util.GetCondition(dtpFDate);
            //dr["TDATE"] = Util.GetCondition(dtpTDate);
            RQSTDT.Rows.Add(dr);

            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService("DA_RTLS_GET_TOUT", "RQSTDT", "RSLTDT", RQSTDT, (dt, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                if (dt.Rows.Count < 1)
                {
                    Util.Alert("SFU1905");
                    return;
                }

                Util.GridSetData(dgRequestList, dt, FrameOperation);
                Util.SetTextBlockText_DataGridRowCount(tbListCount, Util.NVC(dgRequestList.Rows.Count));
            });
        }

        /// <summary>
        /// 선택시 상세 정보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgRequestList_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                int iListIndex = Util.gridFindDataRow(ref dgRequestList, "CHK", "True", false);
                fn_Set_Detail_Request(iListIndex);
            }
            catch (Exception ex)
            {

                Util.AlertError(ex);
            }
        }
        /// <summary>
        /// 불출요청 ID 상세 정보 펑션
        /// </summary>
        /// <param name="iListIndex"></param>
        private void fn_Set_Detail_Request(int iListIndex)
        {
            Util.gridClear(dgRequest_Detail);
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("TOUT_ID", typeof(string));
            RQSTDT.Columns.Add("LANGID", typeof(string));
            DataRow dr = RQSTDT.NewRow();

            dr["TOUT_ID"] = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[iListIndex].DataItem, "TOUT_ID"));
            dr["LANGID"] = LoginInfo.LANGID;

            RQSTDT.Rows.Add(dr);

            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService("DA_RTLS_GET_TOUT_LOT", "RQSTDT", "RSLTDT", RQSTDT, (dt, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                if (dt.Rows.Count < 1)
                {
                    Util.Alert("SFU1905");
                    return;
                }
                
                Util.GridSetData(dgRequest_Detail, dt, FrameOperation);
                Util.SetTextBlockText_DataGridRowCount(tbListDetailCount, Util.NVC(dgRequest_Detail.Rows.Count));
                txtRemark.Text = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[iListIndex].DataItem, "TOUT_RSN_CNTT"));
            });
        }

        /// <summary>
        /// 불출 요청 승인
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //승인 ID 체크
                int iListIndex = fn_Get_Selected_Chk_Index();
                string appUsrID = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[iListIndex].DataItem, "TOUT_APPR_USER_ID"));
                if(!fn_Auth_Chk(appUsrID))
                {
                    Util.AlertInfo("RTLS0002");
                    return;
                }
                //승인하시겠습니까?
                Util.MessageConfirm("SFU2878", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        fn_EDIT_TB_RTLS_TOUT_STAT_CODE("A");
                    }
                });
             
            }
            catch (Exception ex)
            {
                Util.AlertError(ex);
            }
        }

        /// <summary>
        /// 승인 반려 취소 권한 체크
        /// </summary>
        /// <param name="appUsrID"></param>
        /// <returns></returns>
        private bool fn_Auth_Chk(string appUsrID)
        {
            if(LoginInfo.USERID != appUsrID)
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// 불출요청 반려
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //승인 ID 체크
                int iListIndex = fn_Get_Selected_Chk_Index();
                string appUsrID = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[iListIndex].DataItem, "TOUT_APPR_USER_ID"));
                if (!fn_Auth_Chk(appUsrID))
                {
                    Util.AlertInfo("RTLS0004");
                    return;
                }

                //반려하시겠습니까?
                Util.MessageConfirm("SFU2866", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        fn_EDIT_TB_RTLS_TOUT_STAT_CODE("E");
                    }
                });
               
            }
            catch (Exception ex)
            {
                Util.AlertError(ex);
            }

        }

        /// <summary>
        /// 불출요청 취소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                //승인 ID 체크
                int iListIndex = fn_Get_Selected_Chk_Index();
                string appUsrID = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[iListIndex].DataItem, "TOUT_REQ_USER_ID"));
                if (!fn_Auth_Chk(appUsrID))
                {
                    Util.AlertInfo("RTLS0003");
                    return;
                }

                //취소 하시겠습니까?
                Util.MessageConfirm("SFU1243", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        fn_EDIT_TB_RTLS_TOUT_STAT_CODE("C");
                    }
                });
                
            }
            catch (Exception ex)
            {
                Util.AlertError(ex);
            }

        }


        /// <summary>
        /// 불출요청 상태 변환 펑션
        /// </summary>
        /// <param name="statCode"></param>
        private void fn_EDIT_TB_RTLS_TOUT_STAT_CODE(string statCode)
        {
            int iListIndex = fn_Get_Selected_Chk_Index();
            string currentStatcode = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[iListIndex].DataItem, "TOUT_APPR_STAT_CODE"));
            if (!currentStatcode.Equals("R"))
            {
                //불출요청이 생성상태가 아닙니다. 
                Util.Alert("RTLS0001");
                return;
      
            }
 
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("TOUT_ID", typeof(string));
            RQSTDT.Columns.Add("INSUSER", typeof(string));
            RQSTDT.Columns.Add("TOUT_APPR_STAT_CODE", typeof(string));
            RQSTDT.Columns.Add("TOUT_RSN_CNTT", typeof(string));
            DataRow dr = RQSTDT.NewRow();

            dr["TOUT_ID"] = Util.NVC(DataTableConverter.GetValue(dgRequestList.Rows[iListIndex].DataItem, "TOUT_ID"));
            dr["INSUSER"] = LoginInfo.USERID;
            dr["TOUT_APPR_STAT_CODE"] = statCode;
            dr["TOUT_RSN_CNTT"] = txtRemark.Text;
            RQSTDT.Rows.Add(dr);

            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService("BR_RTLS_EDIT_TOUT_REQUEST", "RQSTDT", "", RQSTDT, (dt, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                if (ex != null)
                {
                    Util.MessageException(ex);
                    return ;
                }
                else
                {
                    switch (statCode)
                    {
                        case "A":
                            Util.AlertInfo("SFU1690"); //승인되었습니다.
                            break;
                        case "E":
                            Util.AlertInfo("SFU1541"); //반려되었습니다.
                            break;
                        case "C":
                            Util.AlertInfo("SFU1937"); //취소되었습니다.
                            break;
                    }
                    Util.gridClear(dgRequest_Detail);
                    
                }
            });
            Serarch_List();
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 선택된 불출ID Index Get
        /// </summary>
        /// <returns></returns>
       private int fn_Get_Selected_Chk_Index()
        {
            int index =  Util.gridFindDataRow(ref dgRequestList, "CHK", "True", false);
            if(index < 0)
            {
                throw new Exception(MessageDic.Instance.GetMessage("SFU1636"));
            }
            return index;
        }

        /// <summary>
        /// 그리드 랜더링 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgRequestList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgRequestList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OVER_DATE")) == "") return;

                if (Convert.ToInt64(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OVER_DATE"))) >= 30 &&
                    Convert.ToInt64(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OVER_DATE"))) <= 60)
                {
                    //e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.DataGrid.Columns["STATUSNAME"].Index).Presenter.Foreground = new SolidColorBrush(Colors.Gray);
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                }

                if (Convert.ToInt64(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "OVER_DATE"))) > 60)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                }
            }));
        }
    }
}
