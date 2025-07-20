/*************************************************************************************
 Created Date : 2017.03.20
      Creator : 정문교
   Decription : LOT 정보 변경
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.20  정문교 : Initial Created.
  2019.12.12  오화백 :  LOT Type을 시생산 변경 불가 처리
  2020.01.20  최상민 : lot 유형, 저장 실행시 마감일 체크 추가
  2020.06.18  정문교 : C20200610-000491 - 등급 정보 칼럼 추가
  2022.04.03  장기훈 : [C20220328-000417] - LOT 정보변경시 극성정보를 구분할 수 있는 밸리데이션 추가
  2022.04.26  장기훈 : [C20220328-000417] - LOT 정보변경시 극성정보를 구분할 수 있는 밸리데이션 추가 기능 보완
  2022.11.04  강호운 : [C20221107-000542] - LASER_ABLATION 공정추가
  2024.03.12  안유수 : E20240125-001319 LOT 정보 변경 확인 팝업창 추가 
  2025.01.02  안유수 : E20241121-001005 조립(소형, 자동차)의 경우, LANE 수량 변경처리 못하도록 수정
  2025.03.07  황재원 : E20250115-000986 [NERP]시생산 <> 시생산 외 LOT유형으로의 변경 처리 못하도록 수정
  2025.06.02  조범모 : MI_LS_OSS_0186   term 대상에 대해 정보 변경시 ERP실적 송신 안되니 미리 체크
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_071 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();

        private string _areaid;
        private string _procid;
        private string _eqsgid;
        private string _eqptid;
        private string _prodid;
        private int _wipseq;
        private string _wipstat;
        private string _Process_ErpUseYN;
        private string _FP_REF_PROCID;
        private string _PLAN_MNGT_TYPE_CODE;
        private string _INIT_WIP_TYPE_CODE;
        private string _LINE_GROUP_CODE;
        private string _FRST_PRODID;
        private string _WIP_HOLD;

        private DataTable dtbefore = new DataTable();

        public COM001_071()
        {
            InitializeComponent();

            this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        #endregion

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSearch);
            listAuth.Add(btnSelectWO);
            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            CommonCombo combo = new CommonCombo();
            string[] sFilter = { "MKT_TYPE_CODE" };
            combo.SetCombo(cboMarketType, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter);
            combo.SetCombo(cboLotType, CommonCombo.ComboStatus.SELECT);

            if (LoginInfo.CFG_SYSTEM_TYPE_CODE == "A") txtSelectLaneQty.IsEnabled = false; // 조립의경우 LANE 수량 수정 불가

            // 전극 등급 표시여부
            EltrGrdCodeColumnVisible();

            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotTree(txtLotID.Text);
        }
        #endregion

        #region [생성,삭제]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            //2022.04.03  장기훈: [C20220328 - 000417] - LOT 정보변경시 극성정보를 구분할 수 있는 밸리데이션 추가 ---
            if (VadliationPoarity(txtSelectLot.Text, txtSelectWO.Text).Equals("N"))
            {
                return;
            }
            //--------------------------------------------------------------------------------------------------------

            if (VadliationERPEnd().Equals("CLOSE"))
            {
                Util.MessageValidation("SFU3494"); // ERP 생산실적이 마감 되었습니다.
                return;
            }

            //MI_LS_OSS_0186 term 대상에 대해 정보 변경시 ERP실적 송신 안되므로 미리체크
            if (VadliationLotTerm(txtSelectLot.Text).Equals("Y"))
            {
                Util.MessageValidation("100000081");  //해당 LOT은 재공종료 상태입니다.
                return;
            }
            //if (ValidationMoveShopBlk(txtSelectLot.Text) == false)
            //{
            //    return;
            //}

            COM001_071_SAVE_DATA_CHECK wndPopup = new COM001_071_SAVE_DATA_CHECK();
            //wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[12];
                Parameters[0] = dtbefore;
                Parameters[1] = txtSelectLot.Text;
                Parameters[2] = Convert.ToString(DataTableConverter.GetValue(cboLotType.SelectedItem, "CBO_NAME"));//cboLotType.SelectedValue.ToString();
                Parameters[3] = txtSelectWO.Text;
                Parameters[4] = txtSelectWODetail.Text;
                Parameters[5] = txtSelectProdid.Text;
                Parameters[6] = txtSelectModelid.Text;
                Parameters[7] = txtSelectProdVer.Text;
                Parameters[8] = txtSelectLaneQty.Value;
                Parameters[9] = cboMarketType.SelectedValue.ToString();
                Parameters[10] = txtUserName.Text;
                Parameters[11] = txtWipNote.Text;

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndSave_Closed);
                //wndPopup.Closed += new EventHandler(wndPopupBizWFLot_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
            // 변경하시겠습니까?
            //Util.MessageConfirm("SFU2875", (result) =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            //        Save();
            //    }
            //});

        }

        private void wndSave_Closed(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.COM001.COM001_071_SAVE_DATA_CHECK wndPopup = sender as LGC.GMES.MES.COM001.COM001_071_SAVE_DATA_CHECK;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    // 변경하시겠습니까?
                    Util.MessageConfirm("SFU2875", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Save();
                        }
                    });
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        #endregion

        #region [LOT 선택]
        private void rbCheck_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
                GetLotInfo((rb.DataContext as DataRowView)?.Row["LOTID"].ToString(), (rb.DataContext as DataRowView)?.Row["PROCID"].ToString());
        }

        ////private void treeViewItem_Click(object sender, SourcedEventArgs e)
        ////{
        ////    C1TreeViewItem item = sender as C1TreeViewItem;

        ////    if (item == null)
        ////        return;

        ////    if (((System.Data.DataRow)item.DataContext).ItemArray.Length > 0)
        ////    {
        ////        GetLotInfo(((System.Data.DataRow)item.DataContext).ItemArray[0].ToString(),
        ////                   ((System.Data.DataRow)item.DataContext).ItemArray[2].ToString());
        ////    }
        ////}

        #endregion

        #region [Wo 일자 변경]
        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.Text = System.DateTime.Now.ToLongDateString();
                    dtPik.SelectedDateTime = System.DateTime.Now;
                    //Util.Alert("SFU1698");      //시작일자 이전 날짜는 선택할 수 없습니다.
                    Util.MessageValidation("SFU1698");
                    //e.Handled = false;
                    return;
                }

            }));
        }
        #endregion

        #region [공정] - W/O 조건
        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetEquipment();
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetProcess();
        }

        #endregion

        #region [Wo 조회]
        private void btnSelectWO_Click(object sender, RoutedEventArgs e)
        {
            GetWorkOrder();
        }
        #endregion

        #region [WO 선택하기]
        private void dgWOChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                DataTable dtWO = DataTableConverter.Convert(dgWO.ItemsSource);

                // 전체 Lot 체크 해제(선택후 다른 행 선택시 그전 check 해제)
                dtWO.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
                dtWO.Rows[idx]["CHK"] = 1;
                dtWO.AcceptChanges();

                //Util.GridSetData(dataGrid, dtLot, null, false);
                dgWO.ItemsSource = DataTableConverter.Convert(dtWO);

                //row 색 바꾸기
                dgWO.SelectedIndex = idx;

                txtSelectWO.Text = Util.NVC(DataTableConverter.GetValue(dgWO.Rows[idx].DataItem, "WOID"));
                txtSelectWODetail.Text = Util.NVC(DataTableConverter.GetValue(dgWO.Rows[idx].DataItem, "WO_DETL_ID"));
                txtSelectProdid.Text = Util.NVC(DataTableConverter.GetValue(dgWO.Rows[idx].DataItem, "PRODID"));
                txtSelectModelid.Text = Util.NVC(DataTableConverter.GetValue(dgWO.Rows[idx].DataItem, "MODLID"));

                _prodid = Util.NVC(DataTableConverter.GetValue(dgWO.Rows[idx].DataItem, "PRODID"));

                if (string.IsNullOrEmpty(DataTableConverter.GetValue(dgWO.Rows[idx].DataItem, "MKT_TYPE_CODE").GetString()))
                {
                    cboMarketType.SelectedIndex = 0;
                }
                else
                {
                    cboMarketType.SelectedValue = Util.NVC(DataTableConverter.GetValue(dgWO.Rows[idx].DataItem, "MKT_TYPE_CODE"));
                }

                cboLotType.SelectedValue = Util.NVC(DataTableConverter.GetValue(dgWO.Rows[idx].DataItem, "LOTTYPE"));

                GetRecipeNo();
            }

        }
        #endregion

        #region [버전 선택하기]
        private void dgVerChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                DataTableConverter.SetValue(dgProdVer.Rows[idx].DataItem, "CHK", true);
                //row 색 바꾸기
                dgProdVer.SelectedIndex = idx;

                txtSelectProdVer.Text = Util.NVC(DataTableConverter.GetValue(dgProdVer.Rows[idx].DataItem, "PROD_VER_CODE"));
            }

        }
        #endregion

        #region [LOT ID]
        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotTree(txtLotID.Text);
            }
        }
        #endregion

        #region [요청자]
        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }
        #endregion

        #region [공정 체크박스]
        private void chkProc_Checked(object sender, RoutedEventArgs e)
        {
            cboEquipment.IsEnabled = false;
        }

        private void chkProc_Unchecked(object sender, RoutedEventArgs e)
        {
            cboEquipment.IsEnabled = true;
        }
        #endregion

        #region [LOT ADAPT]
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSelectLot.Text))
            {
                Util.MessageInfo("SFU1364");    // LOT ID가 선택되지 않았습니다.
                return;
            }

            if (VadliationERPEnd().Equals("CLOSE"))
            {
                Util.MessageValidation("SFU3494"); // ERP 생산실적이 마감 되었습니다.
                return;
            }

            COM001_071_ADAPT wndPopup = new COM001_071_ADAPT();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = Util.NVC(txtSelectLot.Text);

                // 트레이 관리 LOT 여부
                // 와인딩 초소형 OUT LOT, 와싱OUT LOT
                if ((Process.WINDING.Equals(_procid) && "CS".Equals(_LINE_GROUP_CODE) && "OUT".Equals(_INIT_WIP_TYPE_CODE))
                    || (Process.WASHING.Equals(_procid) && "OUT".Equals(_INIT_WIP_TYPE_CODE)))
                {
                    Parameters[1] = "Y";
                }
                else
                {
                    Parameters[1] = "N";
                }


                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopup_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }
        private void wndPopup_Closed(object sender, EventArgs e)
        {
            COM001_071_ADAPT window = sender as COM001_071_ADAPT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }
        #endregion
        #endregion

        #region Mehod
        #region [동별 전극 등급 Visible]  
        private void EltrGrdCodeColumnVisible()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.TableName = "RQSTDT";
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "ELTR_GRD_JUDG_ITEM_CODE";
                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_AREA_COM_CODE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    tbEltrGrdCode.Visibility = Visibility.Visible;
                    txtEltrGrdCode.Visibility = Visibility.Visible;
                }
                else
                {
                    tbEltrGrdCode.Visibility = Visibility.Collapsed;
                    txtEltrGrdCode.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Tree 목록 가져오기]
        public void GetLotTree(string slotid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(slotid))
                {
                    // 조회할 LOT ID 를 입력하세요.
                    Util.MessageValidation("SFU1190");
                    return;
                }

                ShowLoadingIndicator();
                DoEvents();

                SetClearLotInfo();
                ////trvLotTrace.Items.Clear();

                #region Lot 정보 조회와 동일하게 변경
                ////DataTable inTable = new DataTable();
                ////inTable.Columns.Add("LOTID", typeof(string));

                ////DataRow dr = inTable.NewRow();
                ////dr["LOTID"] = slotid;

                ////inTable.Rows.Add(dr);

                ////DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTRACE_FOR_SM", "RQSTDT", "RSLTDT", inTable);

                ////if (dtResult != null && dtResult.Rows.Count > 0)
                ////{
                ////    // 모델 또는 Lane 차이 체크
                ////    //dtResult.Columns.Add("DIFF", typeof(string));

                ////    //for (int nrow = 0; nrow < dtResult.Rows.Count; nrow++)
                ////    //{
                ////    //    if (nrow > 0)
                ////    //    {
                ////    //        if (dtResult.Rows[nrow - 1]["MODLID"].ToString() != dtResult.Rows[nrow]["MODLID"].ToString() ||
                ////    //            dtResult.Rows[nrow - 1]["LANE_QTY"].ToString() != dtResult.Rows[nrow]["LANE_QTY"].ToString())
                ////    //            dtResult.Rows[nrow]["DIFF"] = "Y";
                ////    //    }
                ////    //}
                ////    //dtResult.AcceptChanges();

                ////    SetLotTree(trvLotTrace, null, dtResult);
                ////}
                ////else
                ////{
                ////    // 재공정보에서 LOT 정보 검색
                ////    inTable = new DataTable();
                ////    inTable.TableName = "RQSTDT";
                ////    inTable.Columns.Add("LOTID", typeof(string));

                ////    dr = inTable.NewRow();
                ////    dr["LOTID"] = slotid;

                ////    inTable.Rows.Add(dr);

                ////    DataTable dtResult2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP", "RQSTDT", "RSLTDT", inTable);

                ////    if (dtResult2 != null && dtResult2.Rows.Count > 0)
                ////    {
                ////        DataTable dttree = new DataTable();
                ////        dttree.Columns.Add("LOTID", typeof(string));
                ////        dttree.Columns.Add("FROM_LOTID", typeof(string));
                ////        dttree.Columns.Add("PROCID", typeof(string));
                ////        dttree.Columns.Add("MODLID", typeof(string));
                ////        dttree.Columns.Add("LANE_QTY", typeof(Int32));
                ////        dttree.Columns.Add("LOT_PATH", typeof(string));

                ////        DataRow drtree = dttree.NewRow();
                ////        drtree["LOTID"] = dtResult2.Rows[0]["LOTID"];
                ////        drtree["PROCID"] = dtResult2.Rows[0]["PROCID"];
                ////        dttree.Rows.Add(drtree);

                ////        SetLotTree(trvLotTrace, null, dttree);
                ////    }

                ////}
                #endregion

                DataSet inData = new DataSet();
                DataTable dtRqst = inData.Tables.Add("INDATA");

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("GUBUN", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = slotid;
                dr["GUBUN"] = (bool)rdoForward.IsChecked ? "F" : "R";

                dtRqst.Rows.Add(dr);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_LOT_INFO_END", "INDATA", "LOTSTATUS,TREEDATA", inData);

                dsRslt.Relations.Add("Relations", dsRslt.Tables["TREEDATA"].Columns["LOTID"], dsRslt.Tables["TREEDATA"].Columns["FROM_LOTID"]);
                DataView dvRootNodes;
                dvRootNodes = dsRslt.Tables["TREEDATA"].DefaultView;
                dvRootNodes.RowFilter = "FROM_LOTID IS NULL";

                trvLotTrace.ItemsSource = dvRootNodes;
                TreeItemExpandAll();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        #endregion

        #region [LOT 정보 가져오기]
        public void GetLotInfo(string sLot, String sProcID)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                SetClearLotInfo();

                //string Stap = string.Empty;
                DataTable inTable = new DataTable();
                inTable.TableName = "RQSTDT";
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLot;
                dr["PROCID"] = sProcID;

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO", "RQSTDT", "RSLTDT", inTable);

                dtbefore = dtResult;

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    SetLotInfo(dtResult.Rows[0]);
                    GetWorkOrder();
                    GetRecipeNo();
                    getSerachLot(dtResult);
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
        }
        #endregion

        #region LOT 정보 가져오기
        private void getSerachLot(DataTable dtResult)
        {
            _FRST_PRODID = dtResult.Rows[0]["PRODID"].ToString();
            _WIP_HOLD = dtResult.Rows[0]["WIPHOLD"].ToString();
        }
        #endregion
        #region [Process 정보 가져오기]
        private void GetProcessFPInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_PROCESS_FP_INFO();

                DataRow dr = inTable.NewRow();
                dr["PROCID"] = _procid;

                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_FP_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null || dtRslt.Rows.Count > 0)
                {
                    _Process_ErpUseYN = Util.NVC(dtRslt.Rows[0]["ERPRPTIUSE"]);
                    _FP_REF_PROCID = Util.NVC(dtRslt.Rows[0]["FP_REF_PROCID"]);
                    _PLAN_MNGT_TYPE_CODE = Util.NVC(dtRslt.Rows[0]["PLAN_MNGT_TYPE_CODE"]);
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
        }
        #endregion

        #region [Process 정보 가져오기]
        private void SetProcess()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment?.SelectedValue.GetString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";

                //DataRow drIns = dtResult.NewRow();
                //drIns["CBO_NAME"] = "-SELECT-";
                //drIns["CBO_CODE"] = "SELECT";
                //dtResult.Rows.InsertAt(drIns, 0);

                cboProcess.ItemsSource = dtResult.Copy().AsDataView();

                var query = (from t in dtResult.AsEnumerable()
                             where t.Field<string>("CBO_CODE") == _procid
                             select t).FirstOrDefault();

                if (query != null)
                {
                    cboProcess.SelectedValue = _procid;
                }
                else
                {
                    if (dtResult.Rows.Count > 0)
                        cboProcess.SelectedIndex = 0;
                    else if (dtResult.Rows.Count == 0)
                        cboProcess.SelectedItem = null;
                }
                //cboProcess.SelectedValue = _procid;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void SetEquipmentSegment()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.TableName = "RQSTDT";
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                //inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = _areaid;
                //dr["PROCID"] = processCode;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", inTable);

                cboEquipmentSegment.DisplayMemberPath = "CBO_NAME";
                cboEquipmentSegment.SelectedValuePath = "CBO_CODE";

                cboEquipmentSegment.ItemsSource = dtResult.Copy().AsDataView();
                cboEquipmentSegment.SelectedValue = _eqsgid;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [설비 정보 가져오기]
        private void SetEquipment()
        {
            try
            {
                DataTable RQSTDT = new DataTable { TableName = "RQSTDT" };
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = _areaid;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.GetString();
                dr["PROCID"] = cboProcess.SelectedValue.GetString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-SELECT-";
                drIns["CBO_CODE"] = "SELECT";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();
                cboEquipment.SelectedValue = _eqptid;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [WO 정보 가져오기]
        private DataTable GetMixingWorkOrder()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_LIST_BY_PROCID();

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["PROCID"] = _FP_REF_PROCID.Equals("") ? _procid : _FP_REF_PROCID;
                dr["PROCID"] = _FP_REF_PROCID.Equals("") ? cboProcess.SelectedValue.ToString() : _FP_REF_PROCID;
                dr["EQSGID"] = _eqsgid;
                dr["EQPTID"] = cboEquipment.SelectedValue == null ? null : cboEquipment.SelectedValue.ToString();
                dr["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                inTable.Rows.Add(dr);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_WITH_FP_MX", "RQSTDT", "RSLTDT", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private DataTable GetEquipmentWorkOrderByProcWithInnerJoin()
        {
            try
            {
                ShowLoadingIndicator();

                //DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_LIST_BY_PROCID();
                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_SIDE_LIST();

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["PROCID"] = _FP_REF_PROCID.Equals("") ? _procid : _FP_REF_PROCID;
                dr["PROCID"] = _FP_REF_PROCID.Equals("") ? cboProcess.SelectedValue.ToString() : _FP_REF_PROCID;
                dr["EQSGID"] = _eqsgid;
                dr["EQPTID"] = cboEquipment.SelectedValue == null ? null : cboEquipment.SelectedValue.ToString();
                dr["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                if (!string.IsNullOrEmpty(txtTopBack.Text))
                    dr["COAT_SIDE_TYPE"] = txtTopBack.Text;

                inTable.Rows.Add(dr);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_BY_PROCID_WITH_FP", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private DataTable GetEquipmentWorkOrderWithInnerJoin()
        {
            try
            {
                ShowLoadingIndicator();

                //DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_LIST();
                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_SIDE_LIST();

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["PROCID"] = _FP_REF_PROCID.Equals("") ? _procid : _FP_REF_PROCID;
                dr["PROCID"] = _FP_REF_PROCID.Equals("") ? cboProcess.SelectedValue.ToString() : _FP_REF_PROCID;
                //dr["EQSGID"] = _eqsgid;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue?.ToString();
                dr["EQPTID"] = cboEquipment.SelectedValue?.ToString();
                dr["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                if (!string.IsNullOrEmpty(txtTopBack.Text))
                    dr["COAT_SIDE_TYPE"] = txtTopBack.Text;

                inTable.Rows.Add(dr);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_WITH_FP", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private DataTable GetEquipmentWorkOrderByProc()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_LIST_BY_PROCID();

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["PROCID"] = _FP_REF_PROCID.Equals("") ? _procid : _FP_REF_PROCID;
                dr["PROCID"] = _FP_REF_PROCID.Equals("") ? cboProcess.SelectedValue.ToString() : _FP_REF_PROCID;
                dr["EQSGID"] = _eqsgid;
                dr["EQPTID"] = cboEquipment.SelectedValue == null ? null : cboEquipment.SelectedValue.ToString();
                dr["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                inTable.Rows.Add(dr);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_BY_PROCID", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private DataTable GetEquipmentWorkOrder()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_LIST();

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["PROCID"] = _FP_REF_PROCID.Equals("") ? _procid : _FP_REF_PROCID;
                dr["PROCID"] = _FP_REF_PROCID.Equals("") ? cboProcess.SelectedValue.ToString() : _FP_REF_PROCID;
                //dr["EQSGID"] = _eqsgid;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue?.ToString();
                dr["EQPTID"] = cboEquipment.SelectedValue?.ToString();
                dr["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                inTable.Rows.Add(dr);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [버전 정보 가져오기]
        private void GetRecipeNo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["PRODID"] = _prodid;
                dr["AREAID"] = _areaid;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONV_RATE", "INDATA", "RSLTDT", inTable);

                Util.GridSetData(dgProdVer, dtResult, null, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [Lot 정보 변경]
        private void Save()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("WIPSEQ", typeof(decimal));
                inDataTable.Columns.Add("WOID", typeof(string));
                inDataTable.Columns.Add("WO_DETL_ID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("LANE_QTY", typeof(int));
                inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("REQ_USERID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("LOTTYPE", typeof(string));
                inDataTable.Columns.Add("MKT_TYPE_CODE", typeof(string));

                DataRow row = inDataTable.NewRow();

                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["LOTID"] = txtSelectLot.Text;
                row["WIPSEQ"] = _wipseq;
                row["WOID"] = txtSelectWO.Text;
                row["WO_DETL_ID"] = txtSelectWODetail.Text;
                row["PRODID"] = txtSelectProdid.Text;
                row["LANE_QTY"] = txtSelectLaneQty.Value;
                row["PROD_VER_CODE"] = txtSelectProdVer.Text;
                row["NOTE"] = txtWipNote.Text;
                row["REQ_USERID"] = Util.NVC(txtUserName.Tag);
                row["USERID"] = LoginInfo.USERID;
                row["LOTTYPE"] = cboLotType.SelectedValue.GetString();
                row["MKT_TYPE_CODE"] = cboMarketType.SelectedValue.GetString();

                inDataTable.Rows.Add(row);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_ACT_REG_MODIFY_LOT_WO_LANE", "INDATA", null, inDataTable);

                //Util.AlertInfo("변경 되었습니다");
                Util.MessageInfo("SFU1166");

                GetLotTree(txtSelectLot.Text);
                SetClearLotInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

        }
        #endregion

        #region [Validation]
        private bool CanSave()
        {
            DataTable dtWO = DataTableConverter.Convert(dgWO.ItemsSource);
            for (int i = 0; i < dtWO.Rows.Count; i++)
            {
                if (dtWO.Rows[i]["CHK"].ToString().Equals("1"))
                {
                    //if (!(_FRST_PRODID.Equals(dtWO.Rows[i]["PRODID"].ToString())) && (txtSelectHold.ToString().Equals("Y")))
                    if (!(_FRST_PRODID.Equals(dtWO.Rows[i]["PRODID"].ToString())))
                    {
                        if (_WIP_HOLD.Equals("Y"))
                        {
                            Util.MessageInfo("SFU7009");
                            return false;
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(txtLotID.Text))
            {
                // 변경 대상이 없습니다. 변경 할 LOT을 선택 하세요.
                Util.MessageValidation("SFU1565");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtWipNote.Text))
            {
                // 사유를 입력하세요.
                Util.MessageValidation("SFU1594");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUserName.Text) || string.IsNullOrWhiteSpace(Util.NVC(txtUserName.Tag)))
            {
                // 요청자를 입력 하세요.
                Util.MessageValidation("SFU3451");
                return false;
            }

            if (cboLotType.SelectedValue == null || cboLotType.SelectedValue.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU4068");
                return false;
            }

            if (cboMarketType.SelectedValue == null || cboMarketType.SelectedValue.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU4371");
                return false;
            }

            return true;
        }
        #endregion

        #region [Func]

        #region [요청자]
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtUserName.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = wndPerson.USERNAME;
                txtUserName.Tag = wndPerson.USERID;
            }
        }
        #endregion

        #region Lot 정보 Clear
        private void SetClearLotInfo()
        {
            txtSelectLot.Text = string.Empty;
            txtSelectWO.Text = string.Empty;
            txtSelectWODetail.Text = string.Empty;
            txtSelectProdid.Text = string.Empty;
            txtSelectModelid.Text = string.Empty;
            txtSelectProdVer.Text = string.Empty;
            txtSelectHold.Text = string.Empty;
            txtSelectLaneQty.Value = 0;
            txtSelectUnit.Text = string.Empty;
            txtSelectOutQty.Value = 0;
            txtSelectDefectQty.Value = 0;
            txtSelectLossQty.Value = 0;
            txtSelectPrdtReqQty.Value = 0;
            //    txtUserName.Text = string.Empty;
            //    txtUserName.Tag = string.Empty;
            //    txtWipNote.Text = string.Empty;
            txtTopBack.Text = string.Empty;
            txtCaldate.Text = string.Empty;
            txtEltrGrdCode.Text = string.Empty;

            _wipseq = 0;
            _procid = string.Empty;
            _eqsgid = string.Empty;
            _eqptid = string.Empty;
            _prodid = string.Empty;
            _areaid = string.Empty;
            _wipstat = string.Empty;
            _Process_ErpUseYN = string.Empty;
            _FP_REF_PROCID = string.Empty;
            _PLAN_MNGT_TYPE_CODE = string.Empty;

            cboProcess.ItemsSource = null;

            cboLotType.SelectedIndex = 0;
            cboMarketType.SelectedIndex = 0;
            chkProc.IsChecked = false;

            Util.gridClear(dgWO);
            Util.gridClear(dgProdVer);
        }
        #endregion

        #region Lot 정보 Setting
        private void SetLotInfo(DataRow dr)
        {
            txtSelectLot.Text = dr["LOTID"].ToString();
            txtSelectWO.Text = dr["WOID"].ToString();
            txtSelectWODetail.Text = dr["WO_DETL_ID"].ToString();
            txtSelectProdid.Text = dr["PRODID"].ToString();
            txtSelectModelid.Text = dr["MODLID"].ToString();
            txtSelectProdVer.Text = dr["PROD_VER_CODE"].ToString();
            txtSelectLaneQty.Value = Util.NVC_Int(dr["LANE_QTY"].ToString());
            txtSelectUnit.Text = dr["UNIT_CODE"].ToString();
            txtSelectOutQty.Value = Convert.ToDouble(dr["WIPQTY_ED"].ToString());
            txtSelectDefectQty.Value = Convert.ToDouble(dr["CNFM_DFCT_QTY"].ToString());
            txtSelectLossQty.Value = Convert.ToDouble(dr["CNFM_LOSS_QTY"].ToString());
            txtSelectPrdtReqQty.Value = Convert.ToDouble(dr["CNFM_PRDT_REQ_QTY"].ToString());
            txtTopBack.Text = dr["COATING_SIDE_TYPE_CODE"].ToString();
            txtSelectHold.Text = dr["WIPHOLD"].ToString();
            //txtMktTypeCode.Text = dr["MKT_TYPE_CODE"].ToString();
            cboMarketType.SelectedValue = dr["MKT_TYPE_CD"].GetString();
            cboLotType.SelectedValue = dr["LOTTYPE"].GetString();
            txtCaldate.Text = dr["CALDATE"].ToString();
            txtEltrGrdCode.Text = dr["ELTR_GRD_CODE"].ToString();

            //txtUserName.Text = dr["WRK_USER_NAME"].ToString();
            //txtUserName.Tag = dr["WRK_USERID"].ToString();

            _wipseq = Util.NVC_Int(dr["WIPSEQ"].ToString());
            _areaid = dr["AREAID"].ToString();
            _procid = dr["PROCID"].ToString();
            _eqsgid = dr["EQSGID"].ToString();
            _eqptid = dr["EQPTID"].ToString();
            _prodid = dr["PRODID"].ToString();
            _wipstat = dr["WIPSTAT"].ToString();


            _INIT_WIP_TYPE_CODE = dr["INIT_WIP_TYPE_CODE"].ToString();
            _LINE_GROUP_CODE = dr["LINE_GROUP_CODE"].ToString();

            //cboEquipmentSegment.SelectedItemChanged -= cboEquipmentSegment_SelectedItemChanged;
            SetEquipmentSegment();
            //cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;

            //공정,설비 셋팅
            //cboProcess.SelectedItemChanged -= cboProcess_SelectedItemChanged;
            //SetProcess();
            //cboProcess.SelectedItemChanged += cboProcess_SelectedItemChanged;

            //cboEquipmentSegment.SelectedItemChanged -= cboEquipmentSegment_SelectedItemChanged;
            //SetEquipmentSegment(_procid);
            //cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;

            //SetEquipment(_procid);

            Util.gridClear(dgWO);
            Util.gridClear(dgProdVer);
        }
        #endregion

        #region WO 정보 조회
        public void GetWorkOrder()
        {
            //dgWOChoice.d

            Util.gridClear(dgWO);

            // Process 정보 조회
            GetProcessFPInfo();
            // WO Grid 공정별 칼럼 Visibility
            InitializeGridColumns();

            DataTable dtResult = new DataTable();

            if (_PLAN_MNGT_TYPE_CODE.Equals("WO"))  // ERP 실적 전송인 경우는 Workorder Inner Join..
            {
                if (_procid.Equals(Process.MIXING))
                {
                    dtResult = GetMixingWorkOrder();
                }
                else
                {
                    if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                        dtResult = GetEquipmentWorkOrderByProcWithInnerJoin();
                    else
                        dtResult = GetEquipmentWorkOrderWithInnerJoin();
                }

                cboMarketType.IsEnabled = false;

                //노칭, 와인딩 투입 대기인 경우 LOT 타입 변경 가능하도록
                //노칭, 와인딩의 경우 전극이 투입되는 첫 공정이기 때문에 전극의 WO 를 변경할 수 없음
                //생산LOT 타입에 따라 투입 가능한 LOT 타입이 정해지는데 투입하지 못하는 LOT 타입의 전극일 경우 투입가능하도록 하기위해
                //조립에서는 WO 변경을 하지 못하기 때문에 LOT 타입 변경 가능하도록 프로그램 수정ㄹ함
                if ("WAIT".Equals(_wipstat) && (Process.NOTCHING.Equals(_procid) || Process.WINDING.Equals(_procid)))
                {
                    cboLotType.IsEnabled = true;
                    btnLotTypeChange.IsEnabled = true;
                }
                else
                {
                    cboLotType.IsEnabled = false;
                    btnLotTypeChange.IsEnabled = false;
                }
            }
            else
            {
                if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                    dtResult = GetEquipmentWorkOrderByProc();
                else
                    dtResult = GetEquipmentWorkOrder();

                cboMarketType.IsEnabled = true;
                cboLotType.IsEnabled = true;
                btnLotTypeChange.IsEnabled = true;
            }

            if (dtResult == null || dtResult.Rows.Count < 1)
                return;

            // 현재 선택된 W/O CHK = false로 UPDATE
            DataRow[] drUpdate = dtResult.Select("CHK = 1");

            if (drUpdate.Length > 0)
                drUpdate[0]["CHK"] = 0;

            Util.GridSetData(dgWO, dtResult, FrameOperation, true);

            if (string.IsNullOrWhiteSpace(txtSelectWO.Text))
            {
                int idx = _Util.GetDataGridRowIndex(dgWO, "WOID", txtSelectWO.Text);

                if (idx >= 0)
                {
                    DataTableConverter.SetValue(dgWO.Rows[idx].DataItem, "CHK", true);
                    dgWO.SelectedIndex = idx;
                }
            }

            // 공정 조회인 경우 설비 정보 Visible 처리.
            if (chkProc.IsChecked.HasValue && (bool)chkProc.IsChecked)
                dgWO.Columns["EQPTNAME"].Visibility = Visibility.Visible;
            else
                dgWO.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;

        }
        #endregion

        #region WO 공정별 칼럼 Visibility
        private void InitializeGridColumns()
        {
            if (dgWO == null)
                return;

            if (string.IsNullOrWhiteSpace(txtSelectLot.Text))
                return;

            /*
             * C/Roll, S/Roll, Lane수 적용 공정.
             *     C/ROLL = PLAN_QTY(S/ROLL) / LANE_QTY
             * E2000  - TOP_COATING
             * E2300  - INS_COATING
             * E2500  - HALF_SLITTING
             * E3000  - ROLL_PRESSING
             * E3500  - TAPING
             * E3800  - REWINDER
             * E3300  - LASER_ABLATION [C20221107-000542]
             * E3900  - BACK_WINDER
             */
            if (_procid.Equals(Process.TOP_COATING) ||
                _procid.Equals(Process.INS_COATING) ||
                _procid.Equals(Process.HALF_SLITTING) ||
                _procid.Equals(Process.ROLL_PRESSING) ||
                _procid.Equals(Process.TAPING) ||
                _procid.Equals(Process.REWINDER) ||
                _procid.Equals(Process.LASER_ABLATION) ||
                _procid.Equals(Process.BACK_WINDER))
            {
                if (dgWO.Columns.Contains("C_ROLL_QTY"))
                    dgWO.Columns["C_ROLL_QTY"].Visibility = Visibility.Visible;

                if (dgWO.Columns.Contains("S_ROLL_QTY"))
                    dgWO.Columns["S_ROLL_QTY"].Visibility = Visibility.Visible;

                if (dgWO.Columns.Contains("LANE_QTY"))
                    dgWO.Columns["LANE_QTY"].Visibility = Visibility.Visible;

                if (dgWO.Columns.Contains("INPUT_QTY"))
                    dgWO.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
            }
            else
            {
                if (dgWO.Columns.Contains("C_ROLL_QTY"))
                    dgWO.Columns["C_ROLL_QTY"].Visibility = Visibility.Collapsed;

                if (dgWO.Columns.Contains("S_ROLL_QTY"))
                    dgWO.Columns["S_ROLL_QTY"].Visibility = Visibility.Collapsed;

                if (dgWO.Columns.Contains("LANE_QTY"))
                    dgWO.Columns["LANE_QTY"].Visibility = Visibility.Collapsed;

                if (dgWO.Columns.Contains("INPUT_QTY"))
                    dgWO.Columns["INPUT_QTY"].Visibility = Visibility.Visible;
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

        public void TreeItemExpandAll()
        {
            IList<DependencyObject> items = new List<DependencyObject>();
            VTreeHelper.GetChildrenOfType(trvLotTrace, typeof(C1TreeViewItem), ref items);

            foreach (C1TreeViewItem item in items)
            {
                TreeItemExpandNodes(item);
            }
        }

        public void TreeItemExpandNodes(C1TreeViewItem item)
        {
            item.IsExpanded = true;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                IList<DependencyObject> items = new List<DependencyObject>();
                VTreeHelper.GetChildrenOfType(item, typeof(C1TreeViewItem), ref items);
                foreach (C1TreeViewItem childItem in items)
                {
                    TreeItemExpandNodes(childItem);
                }
            }));
        }


        ////private void SetLotTree(ItemsControl control, string FROM_LOTID, DataTable dtLotTrace)
        ////{
        ////    DataRow[] dr;

        ////    if (string.IsNullOrWhiteSpace(FROM_LOTID))
        ////        dr = dtLotTrace.Select("LOTID='" + dtLotTrace.Rows[0]["LOTID"].ToString() + "'");
        ////    else
        ////        dr = dtLotTrace.Select("FROM_LOTID='" + FROM_LOTID + "'");

        ////    foreach (DataRow childLot in dr)
        ////    {
        ////        C1TreeViewItem treeViewItem = new C1TreeViewItem();

        ////        treeViewItem.Click += treeViewItem_Click;
        ////        treeViewItem.DataContext = childLot;
        ////        treeViewItem.Header = childLot["LOTID"] + "-" + childLot["PROCID"];
        ////        treeViewItem.IsExpanded = true;
        ////        control.Items.Add(treeViewItem);

        ////        C1TreeViewItem bb = control as C1TreeViewItem;

        ////        //// 모델, LANE이 틀린경우
        ////        //if (childLot["DIFF"].ToString().Equals("Y"))
        ////        //    treeViewItem.Foreground = new SolidColorBrush(Colors.Red);

        ////        SetLotTree(treeViewItem, childLot["LOTID"].ToString(), dtLotTrace);
        ////    }
        ////}

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }




        #endregion

        #endregion

        private bool CanLotTypeSave()
        {
            if (cboLotType.SelectedValue == null || cboLotType.SelectedValue.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU4068");  //LOT 유형을 선택하세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtWipNote.Text))
            {
                // 사유를 입력하세요.
                Util.MessageValidation("SFU1594");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUserName.Text) || string.IsNullOrWhiteSpace(Util.NVC(txtUserName.Tag)))
            {
                // 요청자를 입력 하세요.
                Util.MessageValidation("SFU3451");
                return false;
            }

            return true;
        }

        private void LotTypeSave()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("WIPSEQ", typeof(decimal));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("REQ_USERID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("LOTTYPE", typeof(string));

                DataRow row = inDataTable.NewRow();

                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["LOTID"] = txtSelectLot.Text;
                row["WIPSEQ"] = _wipseq;
                row["NOTE"] = txtWipNote.Text;
                row["REQ_USERID"] = Util.NVC(txtUserName.Tag);
                row["USERID"] = LoginInfo.USERID;
                row["LOTTYPE"] = cboLotType.SelectedValue.GetString();
                inDataTable.Rows.Add(row);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_MODIFY_LOTTYPE", "INDATA", null, inDataTable);

                dtbefore.Rows[0]["LOTTYPE"] = cboLotType.SelectedValue.ToString();
                dtbefore.Rows[0]["LOTTYPE_NAME"] = Convert.ToString(DataTableConverter.GetValue(cboLotType.SelectedItem, "CBO_NAME"));//파라메타 datatable update

                Util.MessageInfo("SFU1166");    //변경되었습니다.
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void LotLotTypeChange_Click(object sender, RoutedEventArgs e)
        {

            if (!CanLotTypeSave())
            {
                return;
            }

            if (VadliationERPEnd().Equals("CLOSE"))
            {
                Util.MessageValidation("SFU3494"); // ERP 생산실적이 마감 되었습니다.
                return;
            }

            object[] param = new object[] { dtbefore.Rows[0]["LOTTYPE_NAME"], Convert.ToString(DataTableConverter.GetValue(cboLotType.SelectedItem, "CBO_NAME")) };

            // 변경하시겠습니까?
            Util.MessageConfirm("SFU5687", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    LotTypeSave(); 
                }
            }, param);
        }

        //private bool ValidationMoveShopBlk(string sLOTID)
        //{
        //    bool bReturn = false;
        //    DataTable dt = new DataTable();
        //    dt.Columns.Add("LOTID", typeof(string));

        //    DataRow dr = dt.NewRow();
        //    dr["LOTID"] = sLOTID;
        //    dt.Rows.Add(dr);

        //    new ClientProxy().ExecuteService("BR_ACT_CHK_MOVE_SHOP_BLK", "INDATA", "OUTDATA", dt, (bizResult, bizException) =>
        //    {
        //        try
        //        {
        //            if (bizException != null)
        //            {
        //                Util.MessageException(bizException);
        //                return;
        //            }
        //            else
        //                bReturn = true;

        //        }
        //        catch (Exception ex)
        //        {
        //            Util.MessageException(ex);
        //        }
        //        finally
        //        {

        //        }
        //    }
        //    );

        //    return bReturn;
        //}

        private string VadliationERPEnd()
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("WRKDATE", typeof(string));

            DataRow dr = dt.NewRow();
            dr["AREAID"] = _areaid;
            dr["WRKDATE"] = txtCaldate.Text;
            dt.Rows.Add(dr);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ERP_CLOSE", "RQSTDT", "RSLT", dt);

            if (result.Rows.Count != 0)
            {
                return Convert.ToString(result.Rows[0]["ERP_CLOSING_FLAG"]);
            }

            return "OPEN";
        }

        //2022.04.03  장기훈 : [C20220328-000417] - LOT 정보변경시 극성정보를 구분할 수 있는 밸리데이션 추가
        private string VadliationPoarity(string sLOTID, string sWOID)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("WOID", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LOTID"] = sLOTID;
            dr["WOID"] = sWOID;
            dt.Rows.Add(dr);

            DataTable result = new ClientProxy().ExecuteServiceSync("BR_ACT_CHK_MODIFY_POLARITY", "INDATA", "OUTDATA", dt);

            if (result.Rows.Count != 0)
            {
                if (Convert.ToString(result.Rows[0]["TYPE_CHK"]) == "N")
                {
                    Util.MessageValidation(Convert.ToString(result.Rows[0]["MSGID"]));
                }

                return Convert.ToString(result.Rows[0]["TYPE_CHK"]);
            }

            return "N";
        }

        private void cboLotType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboLotType.SelectedValue.ToString() == "X")
            {
                btnLotTypeChange.IsEnabled = false;
				btnSave.IsEnabled = false;
            }
            else
            {
                btnLotTypeChange.IsEnabled = true;
				btnSave.IsEnabled = true;
			}
        }

        private string VadliationLotTerm(string sLOTID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LOTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();

            dr["LOTID"] = sLOTID;

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOT", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult.Rows.Count > 0)
                return dtResult.Rows[0]["LOTTERM"].ToString();
            else
                return null;
        }
    }
}
