/*************************************************************************************
 Created Date : 2017.07.28
      Creator : 오화백
   Decription : QMS 검사의뢰
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.28  오화백 : Initial Created.
  2018.03.07  오화백 : 검사의뢰(대차) - TAB 추가
  2020.03.16  이상훈 : C20190613_17057 검사의뢰(Tray 선택) 탭 추가
                       오창(A010) 및 초소형 제외 함
                       동 정보 기준으로 활성화 라인를 지정하도록 코딩함(향후 변경 필요함)
  2023.10.06  이병윤 : E20230612-000036 can deselect line for query,
                       Inspection Request History, including N4/5/6 POUCH
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
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_095 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
       
        string Procid = string.Empty;
        string Procid_Ncr = string.Empty;
        string MCC_MCS = string.Empty;
        public COM001_095()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
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
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();
            #region 검사의뢰(Pallet)  
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegmentRequest };
            _combo.SetCombo(cboAreaRequest, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaChild);
            
            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboAreaRequest };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcessRequest };
            _combo.SetCombo(cboEquipmentSegmentRequest, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
                    
            C1ComboBox[] cbProcessParent = { cboEquipmentSegmentRequest };
            String[] sFilterProcess = { "SEARCH", "Y", string.Empty, string.Empty };
            _combo.SetCombo(cboProcessRequest, CommonCombo.ComboStatus.SELECT, sFilter: sFilterProcess, sCase: "PROCESS_SORT", cbParent: cbProcessParent);


            //물품청구코드
            C1ComboBox[] cboReqCodeParent = {cboAreaRequest, cboProcessRequest};
            _combo.SetCombo(cboReqCode, CommonCombo.ComboStatus.SELECT, sCase: "REQ_CODE",  cbParent: cboReqCodeParent);
            //if(cboReqCode.SelectedValue.ToString() != "SELECT")
            //{
            //    GetCostNtr();
            //}

            // 의뢰구분
            String[] sFilterQmsRequest = { "", "QMS_INSP_MCLASS_CODE" };
            _combo.SetCombo(cboInspectReq, CommonCombo.ComboStatus.SELECT, sFilter: sFilterQmsRequest, sCase: "QMS_INSPEC_COMMCODES");

            txtUserNameCr.Text = LoginInfo.USERNAME;
            txtUserNameCr.Tag = LoginInfo.USERID;
            #endregion

            #region 검사의뢰(대차)  

            //동
            C1ComboBox[] cboAreaChild_Ctnr = { cboProcessRequest_Ctnr };
            _combo.SetCombo(cboAreaRequest_Ctnr, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild_Ctnr, sCase: "AREA");
            cboAreaRequest_Ctnr.SelectedValue = LoginInfo.CFG_AREA_ID;
            cboAreaRequest_Ctnr.IsEnabled = false;

            //공정
            C1ComboBox[] cboProcessParent_Ctnr = { cboAreaRequest_Ctnr };
            C1ComboBox[] cboProcessChild_Ctnr = { cboEquipmentSegmentRequest_Ctnr };
            _combo.SetCombo(cboProcessRequest_Ctnr, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS", cbParent: cboProcessParent_Ctnr, cbChild: cboProcessChild_Ctnr);

            //라인
            C1ComboBox[] cboLineParent_Ctnr = { cboAreaRequest_Ctnr, cboProcessRequest_Ctnr };
            _combo.SetCombo(cboEquipmentSegmentRequest_Ctnr, CommonCombo.ComboStatus.ALL, sCase: "PROCESS_EQUIPMENT",  cbParent: cboLineParent_Ctnr);
            cboEquipmentSegmentRequest_Ctnr.SelectedValue = LoginInfo.CFG_EQSG_ID;

            //물품청구코드
            C1ComboBox[] cboReqCodeParent_Ctnr = { cboAreaRequest_Ctnr, cboProcessRequest_Ctnr };
            _combo.SetCombo(cboReqCode_Ctnr, CommonCombo.ComboStatus.SELECT, sCase: "REQ_CODE", cbParent: cboReqCodeParent_Ctnr);

            // 의뢰구분
            String[] sFilterQmsRequest_Ctnr = { "", "QMS_INSP_MCLASS_CODE" };
            _combo.SetCombo(cboInspectReq_Ctnr, CommonCombo.ComboStatus.SELECT, sFilter: sFilterQmsRequest_Ctnr, sCase: "QMS_INSPEC_COMMCODES");

            txtUserNameCr_Ctnr.Text = LoginInfo.USERNAME;
            txtUserNameCr_Ctnr.Tag = LoginInfo.USERID;


            #endregion

            #region 검사의뢰(Tray)  
            //동
            C1ComboBox[] cboAreaChild_Tray = { cboEquipmentSegmentRequest_Tray };
            _combo.SetCombo(cboAreaRequest_Tray, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaChild_Tray);


            //라인
            C1ComboBox[] cboEquipmentSegmentParent_Tray = { cboAreaRequest_Tray };
            _combo.SetCombo(cboEquipmentSegmentRequest_Tray, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM",  cbParent: cboEquipmentSegmentParent_Tray);


            // 의뢰구분
            String[] sFilterQmsRequest_Tray = { "", "QMS_INSP_MCLASS_CODE" };
            _combo.SetCombo(cboInspectReq_Tray, CommonCombo.ComboStatus.SELECT, sFilter: sFilterQmsRequest_Tray, sCase: "QMS_INSPEC_COMMCODES");

            txtUserName_Tray.Text = LoginInfo.USERNAME;
            txtUserName_Tray.Tag = LoginInfo.USERID;
            #endregion

            #region 검사의뢰이력

            #region 검사의뢰(Tray_선택)  
            //동
            C1ComboBox[] cboAreaChild_TraySelect = { cboEquipmentSegmentTray };
            _combo.SetCombo(cboAreaTray, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaChild_TraySelect);


            //라인
            C1ComboBox[] cboEquipmentSegmentParent_TraySelect = { cboAreaTray };
            _combo.SetCombo(cboEquipmentSegmentTray, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbParent: cboEquipmentSegmentParent_TraySelect);



            // 의뢰구분
            String[] sFilterQmsRequest_TraySelect = { "", "QMS_INSP_MCLASS_CODE" };
            _combo.SetCombo(cboInspectReq_TraySelect, CommonCombo.ComboStatus.SELECT, sFilter: sFilterQmsRequest_TraySelect, sCase: "QMS_INSPEC_COMMCODES");

            txtUserName_TraySelect.Text = LoginInfo.USERNAME;
            txtUserName_TraySelect.Tag = LoginInfo.USERID;
            #endregion

            #region 검사의뢰이력

            //동
            C1ComboBox[] cboAreaHistoryChild = { cboEquipmentSegmentHistory };
            _combo.SetCombo(cboAreaHistory, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaHistoryChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentHistoryParent = { cboAreaHistory };
            C1ComboBox[] cboEquipmentSegmentHistoryChild = { cboProcessHistory };
            _combo.SetCombo(cboEquipmentSegmentHistory, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentHistoryChild, cbParent: cboEquipmentSegmentHistoryParent);

            //공정
            //C1ComboBox[] cbProcessHistoryParent = { cboEquipmentSegmentHistory };
            //_combo.SetCombo(cboProcessHistory, CommonCombo.ComboStatus.SELECT, sCase: "PROCESS", cbParent: cbProcessHistoryParent);

            C1ComboBox[] cbProcessHistoryParent = { cboEquipmentSegmentHistory };
            String[] cbProcesshistory = { "SEARCH", "Y", string.Empty, string.Empty };
            _combo.SetCombo(cboProcessHistory, CommonCombo.ComboStatus.ALL, sFilter: cbProcesshistory, sCase: "PROCESS_SORT", cbParent: cbProcessHistoryParent);
            
            // 의뢰구분
            String[] sFilterQmsRequestHistory = { "", "QMS_INSP_MCLASS_CODE" };
            _combo.SetCombo(cboReqHistory, CommonCombo.ComboStatus.ALL, sFilter: sFilterQmsRequestHistory, sCase: "COMMCODES");

            // 판정결과
            String[] sFilterElectype = { "", "INSP_PROG_CODE" };
            _combo.SetCombo(cboJudgResult, CommonCombo.ComboStatus.ALL, sFilter: sFilterElectype, sCase: "COMMCODES");
            //구분
            Set_Combo_Pallet_Tray(cboPalletTray);


            #endregion

            #region 검사의뢰 상세이력
            //동
            C1ComboBox[] cboAreaHistoryChild_Detail = { cboEquipmentSegmentHistory_Detail };
            _combo.SetCombo(cboAreaHistory_Detail, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaHistoryChild_Detail);

            //라인
            C1ComboBox[] cboEquipmentSegmentHistoryParent_Detail = { cboAreaHistory_Detail };
            C1ComboBox[] cboEquipmentSegmentHistoryChild_Detail = { cboProcessHistory_Detail };
            _combo.SetCombo(cboEquipmentSegmentHistory_Detail, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentHistoryChild_Detail, cbParent: cboEquipmentSegmentHistoryParent_Detail);

            //공정
            C1ComboBox[] cbProcessHistoryParent_Detail = { cboEquipmentSegmentHistory_Detail };
            String[] cbProcesshistory_Detail = { "SEARCH", "Y", string.Empty, string.Empty };
            _combo.SetCombo(cboProcessHistory_Detail, CommonCombo.ComboStatus.ALL, sFilter: cbProcesshistory_Detail, sCase: "PROCESS_SORT", cbParent: cbProcessHistoryParent_Detail);

            // 의뢰구분
            String[] sFilterQmsRequestHistory_Detail = { "", "QMS_INSP_MCLASS_CODE" };
            _combo.SetCombo(cboReqHistory_Detail, CommonCombo.ComboStatus.ALL, sFilter: sFilterQmsRequestHistory_Detail, sCase: "COMMCODES");

            // 판정결과
            String[] sFilterElectype_Detail = { "", "INSP_PROG_CODE" };
            _combo.SetCombo(cboJudgResult_Detail, CommonCombo.ComboStatus.ALL, sFilter: sFilterElectype_Detail, sCase: "COMMCODES");
          
            #endregion

        }


        private void InitHistorySpread()
        {
            //이력Spread  셋팅
           

        }

        /// <summary>
        /// C20190613_17057 
        /// 오창 소형만 사용 할 수 있도록 제어함
        /// </summary>
        private void InitTabTraySelect()
        {
            TabTraySelect.Visibility = Visibility.Collapsed;

            if ( LoginInfo.CFG_SHOP_ID.Equals("A010"))
            {
                TabTraySelect.Visibility = Visibility.Visible ;
            }
        }
        #endregion

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnQmsRequest);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
            InitCombo();
            InitHistorySpread();
            InitTabTraySelect();
            this.Loaded -= UserControl_Loaded;
        }
      
         #region Qms 검사의뢰(Pallet)
        private void btnSearchRequest_Click(object sender, RoutedEventArgs e)
        {
            GetLotList(true);
        }
        //Pallet ID 엔터시
        private void txtPalletIdRequest_KeyDown(object sender, KeyEventArgs e)
        {
            

            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLotid = txtPalletIdRequest.Text.Trim();
                    if (dgListInput.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListInput.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListInput.Rows[i].DataItem, "PALLETID").ToString() == sLotid)
                            {
                                dgListInput.SelectedIndex = i;
                                dgListInput.ScrollIntoView(i, dgListInput.Columns["CHK"].Index);
                                txtPalletIdRequest.Focus();
                                txtPalletIdRequest.Text = string.Empty;
                                return;
                            }
                        }
                    }
                    GetLotList(false);
                 
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }


        }
        //QMS 검사 의뢰
        private void btnQmsRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    return;
                }
                //병합 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4140"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                QmsRequest();
                            }
                        });
                  //검사의뢰 하시겠습니까?
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void cboProcessRequest_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo _combo = new CommonCombo();
            //물품청구코드
            C1ComboBox[] cboReqCodeParent = { cboAreaRequest, cboProcessRequest };
            _combo.SetCombo(cboReqCode, CommonCombo.ComboStatus.SELECT, sCase: "REQ_CODE", cbParent: cboReqCodeParent);
        }
        
        private void dgSelectInput_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name.Equals("INSPQTY"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }
                }
            }));
        }
        //사용자 조회
        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {  
            if (e.Key == Key.Enter)
            {
                GetUserWindow("P");
            }
        }
        //사용자 조회 버튼
        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow("P");
        }


        //변경수량 입력

        private void dgSelectInput_KeyUp(object sender, KeyEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid grd = (sender as C1.WPF.DataGrid.C1DataGrid);

            if (grd != null &&
                        grd.CurrentCell != null &&
                        grd.CurrentCell.Column != null && grd.CurrentCell.Column.Name.Equals("INSPQTY"))
            {

                if (Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[grd.CurrentCell.Row.Index].DataItem, "WIPQTY")).Replace(",", "")) < Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[grd.CurrentCell.Row.Index].DataItem, "INSPQTY"))==string.Empty?"0": Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[grd.CurrentCell.Row.Index].DataItem, "INSPQTY"))))
                {
                     Util.MessageValidation("SFU4141"); //검사수량은 재공수량보다 클 수 없습니다.

                    //grd.CurrentCell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("RED"));
                }
                

                decimal inspQtySum = 0;
                for (int i = 0; i < dgSelectInput.Rows.Count; i++)
                {
                    inspQtySum = inspQtySum + Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "INSPQTY"))==string.Empty ? "0": Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "INSPQTY")));
                }

                txtInspQty.Text = String.Format("{0:#,##0}", (Math.Round(inspQtySum, 2)));
            }
        }

        //물품청구 코드 선택시
        private void cboReqCode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboReqCode.SelectedValue != null)
                {
                    GetCostNtr();
                }
                else
                {
                    txtCostCenter.Text = string.Empty;
                    txtCostCenter.Tag = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void btnReSet_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListInput);
            Util.gridClear(dgSelectInput);
            cboInspectReq.SelectedIndex = 0;
            txtUserNameCr.Text = LoginInfo.USERNAME;
            txtUserNameCr.Tag = LoginInfo.USERID;
            txtInspQty.Text = string.Empty;
            txtReson.Text = string.Empty;
            txtCostCenter.Text = string.Empty;
            cboReqCode.SelectedIndex = 0;
        
        }

        #region [대상 선택하기]
        //checked, unchecked 이벤트 사용하면 화면에서 사라지고 나타날때도
        //이벤트가 발생함 click 이벤트 잡아서 사용할것

        private void dgListInput_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dgListInput.GetRowCount() == 0)
            {
                return;
            }

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgListInput.GetCellFromPoint(pnt);

            if (cell == null || cell.Value == null)
            {
                return;
            }
            if (cell.Column.Name != "CHK")
            {
                return;
            }

            int seleted_row = dgListInput.CurrentRow.Index;

            if (DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "CHK").Equals(1))
            {
                Seting_dgSelectInput(seleted_row);
            }
            else
            {
                DataTable dtTo = DataTableConverter.Convert(dgSelectInput.ItemsSource);
                if (dtTo.Rows.Count > 0)
                {
                    if (dtTo.Select("PALLETID = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PALLETID") + "'").Length > 0)
                    {
                        dtTo.Rows.Remove(dtTo.Select("PALLETID = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PALLETID") + "'")[0]);
                        dgSelectInput.ItemsSource = DataTableConverter.Convert(dtTo);

                    }
                }

                if (dgSelectInput.Rows.Count == 0)
                {
                    CommonCombo _combo = new CommonCombo();
                    //물품청구코드
                    String[] sFilterQmsRequest = { "SELECT", "SELECT" };
                    _combo.SetCombo(cboReqCode, CommonCombo.ComboStatus.SELECT, sCase: "REQ_CODE", sFilter: sFilterQmsRequest);
                    GetCostNtr();
                }
            }


        }



        private void dgListInput_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {

                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }


                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #endregion


        #region Qms 검사의뢰(대차)
        //조회 버튼 클릭시
        private void btnSearchRequest_Ctnr_Click(object sender, RoutedEventArgs e)
        {
            GetLotList_Ctnr(true);
        }

        //대차 ID로 조회시 
        private void txt_Ctnr_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLotid = txt_Ctnr.Text.Trim();
                    if (dgListInput_Ctnr.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListInput_Ctnr.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListInput_Ctnr.Rows[i].DataItem, "CTNR_ID").ToString() == sLotid)
                            {
                                dgListInput_Ctnr.SelectedIndex = i;
                                dgListInput_Ctnr.ScrollIntoView(i, dgListInput_Ctnr.Columns["CHK"].Index);
                                txt_Ctnr.Focus();
                                txt_Ctnr.Text = string.Empty;
                                return;
                            }
                        }
                    }
                    GetLotList_Ctnr(false);

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //공정에 대한 물품청구코도 재조회
        private void cboProcessRequest_Ctnr_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo _combo = new CommonCombo();
            //물품청구코드
            C1ComboBox[] cboReqCodeParent = { cboAreaRequest_Ctnr, cboProcessRequest_Ctnr };
            _combo.SetCombo(cboReqCode_Ctnr, CommonCombo.ComboStatus.SELECT, sCase: "REQ_CODE", cbParent: cboReqCodeParent);
        }
        //QMS 검사 대상 선택시
        private void dgListInput_Ctnr_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dgListInput_Ctnr.GetRowCount() == 0)
            {
                return;
            }

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgListInput_Ctnr.GetCellFromPoint(pnt);

            if (cell == null || cell.Value == null)
            {
                return;
            }
            if (cell.Column.Name != "CHK")
            {
                return;
            }

            int seleted_row = dgListInput_Ctnr.CurrentRow.Index;

            if (DataTableConverter.GetValue(dgListInput_Ctnr.Rows[seleted_row].DataItem, "CHK").Equals(1))
            {
                Seting_dgSelectInput_Ctnr(seleted_row);
            }
            else
            {
                DataTable dtTo = DataTableConverter.Convert(dgSelectInput_Ctnr.ItemsSource);
                if (dtTo.Rows.Count > 0)
                {
                    if (dtTo.Select("CTNR_ID = '" + DataTableConverter.GetValue(dgListInput_Ctnr.Rows[seleted_row].DataItem, "CTNR_ID") + "'").Length > 0)
                    {
                        dtTo.Rows.Remove(dtTo.Select("CTNR_ID = '" + DataTableConverter.GetValue(dgListInput_Ctnr.Rows[seleted_row].DataItem, "CTNR_ID") + "'")[0]);
                        dgSelectInput_Ctnr.ItemsSource = DataTableConverter.Convert(dtTo);

                    }
                }

                if (dgSelectInput_Ctnr.Rows.Count == 0)
                {
                    CommonCombo _combo = new CommonCombo();
                    //물품청구코드
                    String[] sFilterQmsRequest = { "SELECT", "SELECT" };
                    _combo.SetCombo(cboReqCode_Ctnr, CommonCombo.ComboStatus.SELECT, sCase: "REQ_CODE", sFilter: sFilterQmsRequest);
                    GetCostNtr_Ctnr();
                }
            }
            //검사의뢰 수량
            decimal inspQtySum = 0;
            for (int i = 0; i < dgSelectInput_Ctnr.Rows.Count; i++)
            {
                inspQtySum = inspQtySum + Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSelectInput_Ctnr.Rows[i].DataItem, "WIPQTY")));
            }
            txtInspQty_Ctnr.Text = String.Format("{0:#,##0}", (Math.Round(inspQtySum, 2))) == "0" ? string.Empty : String.Format("{0:#,##0}", (Math.Round(inspQtySum, 2)));
        }
        //QMS 검사 대상 선택시
        private void dgListInput_Ctnr_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {

                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }


                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //QMS 코드 선택시 Cost 설정
        private void cboReqCode_Ctnr_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboReqCode_Ctnr.SelectedValue != null)
                {
                    GetCostNtr_Ctnr();
                }
                else
                {
                    txtCostCenter_Ctnr.Text = string.Empty;
                    txtCostCenter_Ctnr.Tag = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //초기화
        private void btnReSet_Ctnr_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListInput_Ctnr);
            Util.gridClear(dgSelectInput_Ctnr);
            cboInspectReq_Ctnr.SelectedIndex = 0;
            txtUserNameCr_Ctnr.Text = LoginInfo.USERNAME;
            txtUserNameCr_Ctnr.Tag = LoginInfo.USERID;
            txtInspQty_Ctnr.Text = string.Empty;
            txtReson_Ctnr.Text = string.Empty;
            txtCostCenter_Ctnr.Text = string.Empty;
            cboReqCode_Ctnr.SelectedIndex = 0;

        }


        #endregion





        #region Qms 검사의뢰(Tray)

        private void btnSearchRequest_Tray_Click(object sender, RoutedEventArgs e)
        {
            GetLotList_Tray(true);
        }
        private void txtLotRTDRequest_Tray_KeyDown(object sender, KeyEventArgs e)
        {

            //try
            //{
            //    if (e.Key == Key.Enter)
            //    {
            //        GetLotList_Tray(false);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}

            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLotid = txtLotRTDRequest_Tray.Text.Trim();
                    if (dgListInput_Tray.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListInput_Tray.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListInput_Tray.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                            {
                                //Util.Alert("SFU1504");   //동일한 LOT이 스캔되었습니다.
                                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1504"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                //{
                                //    if (result == MessageBoxResult.OK)
                                //    {
                                //        txtLotRTDRequest_Tray.Focus();
                                //        txtLotRTDRequest_Tray.Text = string.Empty;
                                //    }
                                //});
                                dgListInput_Tray.SelectedIndex = i;
                                txtLotRTDRequest_Tray.Focus();
                                txtLotRTDRequest_Tray.Text = string.Empty;
                                return;
                            }
                        }
                    }
                    GetLotList_Tray(false);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        private void dgSelectInput_Tray_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name.Equals("TRAYID"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }
                }
         
            }));
        }

        private void dgListInput_Tray_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {

                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    //if (e.Cell.Column.Name == "QMSYN")
                    //{
                    //    if (Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["QMSYN"].Index).Value) == "N")
                    //    {
                    //        DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "QMSYN", ObjectDic.Instance.GetObjectName("의뢰불가능"));

                    //    }
                    //    else if (Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["QMSYN"].Index).Value) == "Y")
                    //    {

                    //        DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "QMSYN", ObjectDic.Instance.GetObjectName("의뢰가능"));
                    //    }

                    //}
                    //else
                    //{
                    //    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    //}

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //조립LOT 선택
        private void dgLotTray_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //(grdResult.Children[0] as UcAssyResult).dtpCaldate.SelectedDataTimeChanged -= OndtpCaldate_SelectedDataTimeChanged;
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }
                        //row 색 바꾸기
                        //dgListInput_Tray.SelectedIndex = idx;
                        DataTable dtTo = new DataTable();
                        dtTo.Columns.Add("LOTID", typeof(string));
                        dtTo.Columns.Add("TRAYID", typeof(string));
                        dtTo.Columns.Add("PJT", typeof(string));
                        dtTo.Columns.Add("PRODID", typeof(string));
                        dtTo.Columns.Add("QMSYN", typeof(string));
                        DataRow dr = dtTo.NewRow();


                        foreach (DataColumn dc in dtTo.Columns)
                        {
                            if (dc.ColumnName == "TRAYID")
                            {
                                dr[dc.ColumnName] = string.Empty;
                            }
                            else
                            {
                                dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(dgListInput_Tray.Rows[idx].DataItem, dc.ColumnName));
                            }
                        }
                        dtTo.Rows.Add(dr);
                        dgSelectInput_Tray.ItemsSource = DataTableConverter.Convert(dtTo);
                        //dgSelectInput_Tray.SelectedIndex = 0;
                                              
                       
                        //공정
                        Procid = PROCID_CHK(Util.NVC(DataTableConverter.GetValue(dgSelectInput_Tray.Rows[0].DataItem, "PRODID")));
                        CommonCombo _combo = new CommonCombo();
                        String[] sFilterQmsRequest = { cboAreaRequest_Tray.SelectedValue.ToString(), Procid };
                        _combo.SetCombo(cboReqCode_Tray, CommonCombo.ComboStatus.SELECT, sFilter: sFilterQmsRequest, sCase: "REQ_CODE");
                        if(cboReqCode_Tray.Items.Count == 2)
                        {
                            cboReqCode_Tray.SelectedIndex = 1;
                            GetCostNtr_Tray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {

            }
        }

        private void cboReqCode_Tray_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboReqCode_Tray.SelectedValue != null)
                {
                    GetCostNtr_Tray();
                }
                else
                {
                    txtCostCenter_Tray.Text = string.Empty;
                    txtCostCenter_Tray.Tag = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnUser_Tray_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow_Tray();
        }
        private void txtUserName_Tray_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow_Tray();
            }
        }

        private void btnUserCr_Tray_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow_Tray();
        }

        private void txtInspQty_Tray_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtInspQty_Tray.Text, 0))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

      
        private void btnQmsRequest_Tray_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_Tray())
                {
                    return;
                }
                //검사의뢰하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4140"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                QmsRequest_Tray();
                            }
                        });
                 
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void btnReSet_Tray_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListInput_Tray);
            Util.gridClear(dgSelectInput_Tray);
            cboInspectReq_Tray.SelectedIndex = 0;
            txtUserName_Tray.Text = LoginInfo.USERNAME;
            txtUserName_Tray.Tag = LoginInfo.USERID;
            txtInspQty_Tray.Text = string.Empty;
            txtReson_Tray.Text = string.Empty;
            txtCostCenter_Tray.Text = string.Empty;
            cboReqCode_Tray.SelectedIndex = 0;
        }
        #endregion

        #region 검사의뢰 이력
        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {

            GetList_QMS_REQ_History();
           
        }

        //검사의뢰취소
        private void btnReqCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCel_Validation())
                {
                    return;
                }
                //검사의뢰취소하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4142"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                if (cboPalletTray.SelectedValue.ToString() == "CTNR")
                                {
                                    //대차 의뢰취소
                                    Cancel_QmsRequest_Ctnr();
                                   
                                }
                                else
                                {
                                    Cancel_QmsRequest();
                                }
                        
                          }

                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void cboPalletTray_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
           
            if (cboPalletTray.SelectedValue.ToString() == "TRAY") //트레이
            {
                //cboAreaHistory.IsEnabled = false;
                cboEquipmentSegmentHistory.IsEnabled = false;
                cboProcessHistory.IsEnabled = false;
                txtPjtHistory.IsEnabled = false;
                txtProdID.IsEnabled = false;
                txtLotHistory.IsEnabled = false;
                txtPalletHistory.IsEnabled = false;
                txtCtnr_IDHistory.IsEnabled = false;

                txtPjtHistory.Text = string.Empty;
                txtProdID.Text = string.Empty;
                txtLotHistory.Text = string.Empty;
                txtPalletHistory.Text = string.Empty;
                txtCtnr_IDHistory.Text = string.Empty;

                titPalletHistory.Visibility = Visibility.Visible;
                txtPalletHistory.Visibility = Visibility.Visible;

                titCtnr_IDHIstory.Visibility = Visibility.Collapsed;
                txtCtnr_IDHistory.Visibility = Visibility.Collapsed;

                dgListReqQmsHistory.Columns["TRAYID"].Visibility = Visibility.Visible;
                dgListReqQmsHistory.Columns["LOTID"].Visibility = Visibility.Collapsed;
                dgListReqQmsHistory.Columns["CTNR_ID"].Visibility = Visibility.Collapsed;

                dgSelectInput_History.Columns["CSTID"].Visibility = Visibility.Visible;
                dgSelectInput_History.Columns["LOTID"].Visibility = Visibility.Visible;
                dgSelectInput_History.Columns["PALLETID"].Visibility = Visibility.Collapsed;
                dgSelectInput_History.Columns["CTNR_ID"].Visibility = Visibility.Collapsed;


                txtUserNameCr_History.IsEnabled = false;
                btnUserCr_History.IsEnabled = false;
            }
            else if (cboPalletTray.SelectedValue.ToString() == "CTNR") //대차
            {
                //cboAreaHistory.IsEnabled = true;
                cboEquipmentSegmentHistory.IsEnabled = true;
                cboProcessHistory.IsEnabled = true;
                txtPjtHistory.IsEnabled = true;
                txtProdID.IsEnabled = true;
                txtLotHistory.IsEnabled = true;
                txtPalletHistory.IsEnabled = true;
                txtCtnr_IDHistory.IsEnabled = true;

                txtPjtHistory.Text = string.Empty;
                txtProdID.Text = string.Empty;
                txtLotHistory.Text = string.Empty;
                txtPalletHistory.Text = string.Empty;
                txtCtnr_IDHistory.Text = string.Empty;

                titPalletHistory.Visibility = Visibility.Collapsed;
                txtPalletHistory.Visibility = Visibility.Collapsed;

                titCtnr_IDHIstory.Visibility = Visibility.Visible;
                txtCtnr_IDHistory.Visibility = Visibility.Visible;

                dgListReqQmsHistory.Columns["TRAYID"].Visibility = Visibility.Collapsed;
                dgListReqQmsHistory.Columns["LOTID"].Visibility = Visibility.Collapsed;
                dgListReqQmsHistory.Columns["CTNR_ID"].Visibility = Visibility.Visible;

                dgSelectInput_History.Columns["CSTID"].Visibility = Visibility.Collapsed;
                dgSelectInput_History.Columns["LOTID"].Visibility = Visibility.Collapsed;
                dgSelectInput_History.Columns["PALLETID"].Visibility = Visibility.Collapsed;
                dgSelectInput_History.Columns["CTNR_ID"].Visibility = Visibility.Visible;

                txtUserNameCr_History.IsEnabled = true;
                btnUserCr_History.IsEnabled = true;

            }
            else if (cboPalletTray.SelectedValue.ToString() == "PALLET") //팔레트
            {
                cboEquipmentSegmentHistory.IsEnabled = true;
                cboProcessHistory.IsEnabled = true;
                txtPjtHistory.IsEnabled = true;
                txtProdID.IsEnabled = true;
                txtLotHistory.IsEnabled = true;
                txtPalletHistory.IsEnabled = true;
                txtCtnr_IDHistory.IsEnabled = true;

                txtPjtHistory.Text = string.Empty;
                txtProdID.Text = string.Empty;
                txtLotHistory.Text = string.Empty;
                txtPalletHistory.Text = string.Empty;
                txtCtnr_IDHistory.Text = string.Empty;

                titPalletHistory.Visibility = Visibility.Visible;
                txtPalletHistory.Visibility = Visibility.Visible;

                titCtnr_IDHIstory.Visibility = Visibility.Collapsed;
                txtCtnr_IDHistory.Visibility = Visibility.Collapsed;


                dgListReqQmsHistory.Columns["TRAYID"].Visibility = Visibility.Collapsed;
                dgListReqQmsHistory.Columns["LOTID"].Visibility = Visibility.Visible;
                dgListReqQmsHistory.Columns["CTNR_ID"].Visibility = Visibility.Collapsed;

                dgSelectInput_History.Columns["CSTID"].Visibility = Visibility.Collapsed;
                dgSelectInput_History.Columns["LOTID"].Visibility = Visibility.Collapsed;
                dgSelectInput_History.Columns["PALLETID"].Visibility = Visibility.Visible;
                dgSelectInput_History.Columns["CTNR_ID"].Visibility = Visibility.Collapsed;

                txtUserNameCr_History.IsEnabled = false;
                btnUserCr_History.IsEnabled = false;

            }
        }


        private void dgListReqQmsHistory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if (dg.CurrentColumn.Name.Equals("INSP_REQ_ID") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    GetDetailLot(Util.NVC(DataTableConverter.GetValue(dgListReqQmsHistory.CurrentRow.DataItem, "INSP_REQ_ID")));
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

    

        private void dgListReqQmsHistory_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dgListReqQmsHistory.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //link 색변경
                    if (e.Cell.Column.Name.Equals("INSP_REQ_ID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }
            }));
        }

        private void btnReqCancel_Detail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Detail_CanCel_Validation())
                {
                    return;
                }
                //물청취소하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4143"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Detail_Cancel_QmsRequest();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void dgListReqQmsHistory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgListReqQmsHistory.GetCellFromPoint(pnt);

            if (cell != null)
            {

                try
                {
                    GetDetailLot(Util.NVC(DataTableConverter.GetValue(dgListReqQmsHistory.CurrentRow.DataItem, "INSP_REQ_ID")));
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }


        //이력 의뢰취소자 텍스ㅌ박스
        private void txtUserNameCr_History_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow_History();
            }
        }
        //이력 취소자 버튼
        private void btnUserCr_History_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow_History();
        }

        #endregion

        #region 검사의뢰 상세이력
        private void btnSearchHistory_Detail_Click(object sender, RoutedEventArgs e)
            {
                GetList_QMS_REQ_History_Detail();
            }
            #endregion
       
        #endregion

        #region Mehod

        #region [대상목록 가져오기]

        #region QMS 검사의뢰(Pallet)
        public void GetLotList(bool bButton)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                
                DataRow dr = dtRqst.NewRow();
                if (Util.GetCondition(txtPalletIdRequest).Equals("")) //PalletID 가 없는 경우
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentRequest, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                    if (dr["EQSGID"].Equals("")) return;

                    //dr["PROCID"] = Util.GetCondition(cboProcessHold, "SFU3207"); //공정을선택하세요. >> 선택해주세요.
                    //if (dr["PROCID"].Equals("")) return;
                    dr["PROCID"] = Util.GetCondition(cboProcessRequest, bAllNull: true);

                    dr["PJT_NAME"] = txtPrjtNameRequest.Text;
                    dr["PRODID"] = txtProdidRequest.Text;
                    dr["LOTID_RT"] = txtLotRTDRequest.Text;
                    dr["WIP_QLTY_TYPE_CODE"] = "G";
                    dr["LANGID"] = LoginInfo.LANGID;
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["WIP_QLTY_TYPE_CODE"] = "G";
                    dr["LOTID"] = Util.GetCondition(txtPalletIdRequest);
                    dr["LANGID"] = LoginInfo.LANGID;
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLTE_MERGER_SPLIT_QTY_LIST", "INDATA", "OUTDATA", dtRqst);



                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListInput, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectInput);
                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtPalletIdRequest.Focus();
                            txtPalletIdRequest.Text = string.Empty;
                        }
                    });
                    return;
                }
                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                    if (dtRslt.Rows.Count == 1)
                    {
                        if(dtRslt.Rows[0]["WIP_QLTY_TYPE_CODE"].ToString() != "G")
                        {
                            //QMS 의뢰는 양품만 등록가능합니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4144"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtPalletIdRequest.Focus();
                                    txtPalletIdRequest.Text = string.Empty;
                                }
                            });
                            return;
                        }
                        DataTable dtSource = DataTableConverter.Convert(dgListInput.ItemsSource);
                        dtSource.Merge(dtRslt);
                        Util.gridClear(dgListInput);
                        //dgListInput.ItemsSource = DataTableConverter.Convert(dtSource);
                        Util.GridSetData(dgListInput, dtSource, FrameOperation, true);
                        DataTableConverter.SetValue(dgListInput.Rows[dgListInput.Rows.Count-1].DataItem, "CHK", 1);
                        Seting_dgSelectInput(dgListInput.Rows.Count - 1);
                        txtPalletIdRequest.Focus();
                        txtPalletIdRequest.Text = string.Empty;
                    }
                    else
                    {

                        DataTable dtRqst2 = dtRslt.Select("WIP_QLTY_TYPE_CODE = 'G'").CopyToDataTable();

                        Util.gridClear(dgListInput);
                        Util.GridSetData(dgListInput, dtRqst2, FrameOperation, true);
                    }


                }
                else
                {
                    Util.GridSetData(dgListInput, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectInput);
                 
                }
            
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void GetCostNtr()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("RESNCODE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
               

                DataRow dr = dtRqst.NewRow();
              
                  
                 dr["AREAID"] = Util.GetCondition(cboAreaRequest, bAllNull: true);
                dr["PROCID"] = Procid_Ncr == string.Empty ? Util.GetCondition(cboProcessRequest, bAllNull: true) : Procid_Ncr;           
                 dr["RESNCODE"] = Util.GetCondition(cboReqCode, bAllNull: true);
                 dr["LANGID"] = LoginInfo.LANGID;
               
               
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QMS_REQ_COST_NTR", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    txtCostCenter.Text = dtRslt.Rows[0]["COST_CNTR_NAME"].ToString();
                    txtCostCenter.Tag = dtRslt.Rows[0]["COST_CNTR_ID"].ToString();
                }
                else
                {
                    txtCostCenter.Text = string.Empty;
                    txtCostCenter.Tag = string.Empty;
                }
                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        public void GetLotList_Tag(DataTable Result)
        {
            try
            {
                COM001_095_PRINT popupTagPrint = new COM001_095_PRINT();
                popupTagPrint.FrameOperation = this.FrameOperation;
              
                object[] parameters = new object[4];


                DataTable TagTable = new DataTable();
                TagTable.Columns.Add("CHK", typeof(string));
                TagTable.Columns.Add("LOTID", typeof(string));
                TagTable.Columns.Add("LOTID_RT", typeof(string));
                TagTable.Columns.Add("LOTYNAME", typeof(string));
                TagTable.Columns.Add("PJT", typeof(string));
                TagTable.Columns.Add("PRODID", typeof(string));
                TagTable.Columns.Add("WIPQTY", typeof(decimal));
                TagTable.Columns.Add("UNIT", typeof(string));
                TagTable.Columns.Add("WIPSEQ", typeof(string));
                TagTable.Columns.Add("WIPHOLD", typeof(string));
                TagTable.Columns.Add("PROC_LABEL_PRT_FLAG", typeof(string));
                TagTable.Columns.Add("EQPTID", typeof(string));
                TagTable.Columns.Add("PROCID", typeof(string));
                TagTable.Columns.Add("CLSS3_CODE", typeof(string));
                if (dgSelectInput.Rows.Count > 0)
                {
                    for(int i=0; i< dgSelectInput.Rows.Count; i++)
                    {
                        DataRow drnewrow = TagTable.NewRow();
                        drnewrow["CHK"] =0;
                        drnewrow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "PALLETID"));
                        drnewrow["LOTID_RT"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "LOTID_RT"));
                        drnewrow["LOTYNAME"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "LOTYNAME"));
                        drnewrow["PJT"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "PJT"));
                        drnewrow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "PRODID"));
                        drnewrow["WIPQTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "WIPQTY")).Replace(",","")) - Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "INSPQTY")).Replace(",", ""));
                        drnewrow["UNIT"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "UNIT"));
                        drnewrow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "WIPSEQ"));
                        drnewrow["WIPHOLD"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "WIPHOLD"));
                        drnewrow["PROC_LABEL_PRT_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "PROC_LABEL_PRT_FLAG"));
                        drnewrow["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "EQPTID"));
                        drnewrow["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "PROCID"));
                        drnewrow["CLSS3_CODE"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "CLSS3_CODE"));
                        TagTable.Rows.Add(drnewrow);
                    }

                }
               if (Result.Rows.Count > 0)
                {
                    parameters[0] = "R"; //QMS 검사의뢰
                    parameters[1] = Result.Rows[0]["REQ_ID"].ToString();
                    parameters[2] = txtInspQty.Text.ToString();
                    parameters[3] = TagTable;

                }
                C1WindowExtension.SetParameters(popupTagPrint, parameters);

                //popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);
                popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);

                grdMain.Children.Add(popupTagPrint);
                popupTagPrint.BringToFront();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }
        private void popupTagPrint_Closed(object sender, EventArgs e)
        {
            COM001_095_PRINT popupTagPrint = new COM001_095_PRINT();

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Util.gridClear(dgListInput);
                    Util.gridClear(dgSelectInput);
                    cboInspectReq.SelectedIndex = 0;
                    txtUserNameCr.Text = LoginInfo.USERNAME;
                    txtUserNameCr.Tag = LoginInfo.USERID;
                    txtInspQty.Text = string.Empty;
                    txtReson.Text = string.Empty;
                    txtCostCenter.Text = string.Empty;
                    cboReqCode.SelectedIndex = 0;
                    GetLotList(true);
                }
            });
            //Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            this.grdMain.Children.Remove(popupTagPrint);
        }
            
        public void Seting_dgSelectInput(int seleted_row)
        {
            DataTable dtTo = DataTableConverter.Convert(dgSelectInput.ItemsSource);
            if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTo.Columns.Add("CHK", typeof(Boolean));
                dtTo.Columns.Add("PALLETID", typeof(string));
                dtTo.Columns.Add("LOTID_RT", typeof(string));
                dtTo.Columns.Add("PJT", typeof(string));
                dtTo.Columns.Add("PRODID", typeof(string));
                dtTo.Columns.Add("WIPQTY", typeof(string));
                dtTo.Columns.Add("INSPQTY", typeof(string));
                dtTo.Columns.Add("UNIT", typeof(string));
                dtTo.Columns.Add("UPDDATE", typeof(string));
                dtTo.Columns.Add("PROCID", typeof(string));
                dtTo.Columns.Add("EQSGID", typeof(string));
                dtTo.Columns.Add("QMSYN", typeof(string));
                dtTo.Columns.Add("WIPSEQ", typeof(string));
                dtTo.Columns.Add("WIPHOLD", typeof(string));
                dtTo.Columns.Add("PROC_LABEL_PRT_FLAG", typeof(string));
                dtTo.Columns.Add("EQPTID", typeof(string));
                dtTo.Columns.Add("LOTYNAME", typeof(string));
                dtTo.Columns.Add("CLSS3_CODE", typeof(string));
            }

            if (dgSelectInput.Rows.Count > 0)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PROCID")) != Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "PROCID")))
                {
                    //동일한 공정 데이터가 아닙니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4145"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);
                        }
                    });
                    return;
                }

                if (Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "LOTID_RT")) != Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "LOTID_RT")))
                {
                    //동일한 조립LOT이 아닙니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4146"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);
                        }
                    });
                    return;
                }


            }
            DataRow dr = dtTo.NewRow();
            foreach (DataColumn dc in dtTo.Columns)
            {
                if (dc.DataType.Equals(typeof(Boolean)))
                {
                    dr[dc.ColumnName] = 0;
                }
                else if (dc.ColumnName == "INSPQTY")
                {
                    dr[dc.ColumnName] = string.Empty;
                }
                else
                {
                    dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, dc.ColumnName));
                }
            }
            dtTo.Rows.Add(dr);
            dgSelectInput.ItemsSource = DataTableConverter.Convert(dtTo);
            if (dgSelectInput.Rows.Count == 1)
            {
                CommonCombo _combo = new CommonCombo();
                //물품청구코드
                String[] sFilterQmsRequest = { cboAreaRequest.SelectedValue.ToString(), Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PROCID")) };
                _combo.SetCombo(cboReqCode, CommonCombo.ComboStatus.SELECT, sCase: "REQ_CODE", sFilter: sFilterQmsRequest);
                Procid_Ncr = Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PROCID"));
                GetCostNtr();
            }

        }

        #endregion

        #region QMS 검사의뢰(대차)
        //조회 메소드
        public void GetLotList_Ctnr(bool bButton)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CTNR_ID",  typeof(string));
                dtRqst.Columns.Add("PROCID",   typeof(string));
                dtRqst.Columns.Add("EQSGID",   typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID",   typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LANGID",   typeof(string));


                DataRow dr = dtRqst.NewRow();
                if (Util.GetCondition(txt_Ctnr).Equals("")) //대차ID가 없는 경우
                {
                    dr["PROCID"] = Util.GetCondition(cboProcessRequest_Ctnr, "SFU3207"); //공정을선택하세요. >> 선택해주세요.
                    if (dr["PROCID"].Equals("")) return;
                    //dr["PROCID"] = Util.GetCondition(cboProcessRequest, bAllNull: true);
                    //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentRequest_Ctnr, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                    //if (dr["EQSGID"].Equals("")) return;
                     
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentRequest_Ctnr, bAllNull: true);
                    dr["PJT_NAME"] = txtPrjtNameRequest_Ctnr.Text;
                    dr["PRODID"] = txtProdidRequest_Ctnr.Text;
                    dr["LOTID_RT"] = txtLotRTDRequest_Ctnr.Text;
                    dr["LANGID"] = LoginInfo.LANGID;
                }               
                else //대차ID 가 있는경우 다른 조건 모두 무시
                {
                    dr["CTNR_ID"] = Util.GetCondition(txt_Ctnr);
                    dr["LANGID"] = LoginInfo.LANGID;

                }

                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CTNR_QMS_LIST", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListInput_Ctnr, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectInput_Ctnr);
                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txt_Ctnr.Focus();
                            txt_Ctnr.Text = string.Empty;
                        }
                    });
                    return;
                }
                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                    if (dtRslt.Rows.Count == 1)
                    {
                        
                        DataTable dtSource = DataTableConverter.Convert(dgListInput_Ctnr.ItemsSource);
                        dtSource.Merge(dtRslt);
                        Util.gridClear(dgListInput_Ctnr);
                        Util.GridSetData(dgListInput_Ctnr, dtSource, FrameOperation, true);
                        DataTableConverter.SetValue(dgListInput_Ctnr.Rows[dgListInput_Ctnr.Rows.Count - 1].DataItem, "CHK", 1);
                        Seting_dgSelectInput_Ctnr(dgListInput_Ctnr.Rows.Count - 1);
                        txt_Ctnr.Focus();
                        txt_Ctnr.Text = string.Empty;
                    }
                    else
                    {
                    

                        Util.gridClear(dgListInput_Ctnr);
                        Util.GridSetData(dgListInput_Ctnr, dtRqst, FrameOperation, true);
                    }


                }
                else
                {
                    Util.GridSetData(dgListInput_Ctnr, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectInput_Ctnr);

                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //스프레드 선택시 
        public void Seting_dgSelectInput_Ctnr(int seleted_row)
        {
            DataTable dtTo = DataTableConverter.Convert(dgSelectInput_Ctnr.ItemsSource);
            if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTo.Columns.Add("CHK", typeof(Boolean));
                dtTo.Columns.Add("CTNR_ID", typeof(string));
                dtTo.Columns.Add("LOTID_RT", typeof(string));
                dtTo.Columns.Add("PRJT_NAME", typeof(string));
                dtTo.Columns.Add("PRODID", typeof(string));
                dtTo.Columns.Add("WIPQTY", typeof(string));
                dtTo.Columns.Add("UNIT", typeof(string));
                dtTo.Columns.Add("MKT_TYPE_CODE", typeof(string));
                dtTo.Columns.Add("MKT_TYPE_NAME", typeof(string));
                dtTo.Columns.Add("PROCID", typeof(string));
                dtTo.Columns.Add("EQSGID", typeof(string));

            }

            if (dgSelectInput_Ctnr.Rows.Count > 0)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgListInput_Ctnr.Rows[seleted_row].DataItem, "PROCID")) != Util.NVC(DataTableConverter.GetValue(dgSelectInput_Ctnr.Rows[0].DataItem, "PROCID")))
                {
                    //동일한 공정 데이터가 아닙니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4145"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTableConverter.SetValue(dgListInput_Ctnr.Rows[seleted_row].DataItem, "CHK", 0);
                        }
                    });
                    return;
                }

                if (Util.NVC(DataTableConverter.GetValue(dgListInput_Ctnr.Rows[seleted_row].DataItem, "LOTID_RT")) != Util.NVC(DataTableConverter.GetValue(dgSelectInput_Ctnr.Rows[0].DataItem, "LOTID_RT")))
                {
                    //동일한 조립LOT이 아닙니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4146"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTableConverter.SetValue(dgListInput_Ctnr.Rows[seleted_row].DataItem, "CHK", 0);
                        }
                    });
                    return;
                }

                if (Util.NVC(DataTableConverter.GetValue(dgListInput_Ctnr.Rows[seleted_row].DataItem, "PRODID")) != Util.NVC(DataTableConverter.GetValue(dgSelectInput_Ctnr.Rows[0].DataItem, "PRODID")))
                {
                    //동일한 제품이 아닙니다..
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4178"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTableConverter.SetValue(dgListInput_Ctnr.Rows[seleted_row].DataItem, "CHK", 0);
                        }
                    });
                    return;
                }
                if (Util.NVC(DataTableConverter.GetValue(dgListInput_Ctnr.Rows[seleted_row].DataItem, "EQSGID")) != Util.NVC(DataTableConverter.GetValue(dgSelectInput_Ctnr.Rows[0].DataItem, "EQSGID")))
                {
                    //동일한 제품이 아닙니다..
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4618"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTableConverter.SetValue(dgListInput_Ctnr.Rows[seleted_row].DataItem, "CHK", 0);
                        }
                    });
                    return;
                }
            }
            DataRow dr = dtTo.NewRow();
            foreach (DataColumn dc in dtTo.Columns)
            {
                if (dc.DataType.Equals(typeof(Boolean)))
                {
                    dr[dc.ColumnName] = 0;
                }
                else
                {
                    dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(dgListInput_Ctnr.Rows[seleted_row].DataItem, dc.ColumnName));
                }
            }
            dtTo.Rows.Add(dr);
            dgSelectInput_Ctnr.ItemsSource = DataTableConverter.Convert(dtTo);

            if (dgSelectInput_Ctnr.Rows.Count == 1)
            {
                CommonCombo _combo = new CommonCombo();
                //물품청구코드
                String[] sFilterQmsRequest = { cboAreaRequest_Ctnr.SelectedValue.ToString(), Util.NVC(DataTableConverter.GetValue(dgListInput_Ctnr.Rows[seleted_row].DataItem, "PROCID")) };
                _combo.SetCombo(cboReqCode, CommonCombo.ComboStatus.SELECT, sCase: "REQ_CODE", sFilter: sFilterQmsRequest);
                Procid_Ncr = Util.NVC(DataTableConverter.GetValue(dgListInput_Ctnr.Rows[seleted_row].DataItem, "PROCID"));
                GetCostNtr_Ctnr();
            }

        }

        //코스트 조회
        private void GetCostNtr_Ctnr()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("RESNCODE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));


                DataRow dr = dtRqst.NewRow();


                dr["AREAID"] = Util.GetCondition(cboAreaRequest_Ctnr, bAllNull: true);
                dr["PROCID"] = Procid_Ncr == string.Empty ? Util.GetCondition(cboProcessRequest_Ctnr, bAllNull: true) : Procid_Ncr;
                dr["RESNCODE"] = Util.GetCondition(cboReqCode_Ctnr, bAllNull: true);
                dr["LANGID"] = LoginInfo.LANGID;


                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QMS_REQ_COST_NTR", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    txtCostCenter_Ctnr.Text = dtRslt.Rows[0]["COST_CNTR_NAME"].ToString();
                    txtCostCenter_Ctnr.Tag = dtRslt.Rows[0]["COST_CNTR_ID"].ToString();
                }
                else
                {
                    txtCostCenter_Ctnr.Text = string.Empty;
                    txtCostCenter_Ctnr.Tag = string.Empty;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //사용자 팝업
        private void txtUserNameCr_Ctnr_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow_Ctnr();
            }
        }
        //사용자 팝업
        private void btnUserCr_Ctnr_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow_Ctnr();
        }
        #endregion


        #region QMS 검사의뢰(Tray)
        public void GetLotList_Tray(bool bButton)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                if(Util.GetCondition(txtLotRTDRequest_Tray).Equals(""))
                {
                    Util.AlertInfo("SFU4147"); //조립LOTID를 입력하세요
                    return;
                }
                if(txtLotRTDRequest_Tray.Text.Length < 8)
                {
                    Util.AlertInfo("SFU4148"); //조립LOTID는 8자리 이상입니다.
                    return;
                }
                if (Util.GetCondition(txtLotRTDRequest_Tray).Equals("")) //PalletID 가 없는 경우
                {
                    dr["AREAID"] = Util.GetCondition(cboAreaRequest_Tray, bAllNull: true);
                }
                else
                {
                    dr["LOTID"] = txtLotRTDRequest_Tray.Text;
                }
                dr["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QMS_REQ_LOT_TRAY", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListInput_Tray, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectInput_Tray);
                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtLotRTDRequest_Tray.Focus();
                            txtLotRTDRequest_Tray.Text = string.Empty;
                        }
                    });
                    return;
                }
                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                    if (dtRslt.Rows.Count == 1)
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgListInput_Tray.ItemsSource);
                        dtSource.Merge(dtRslt);
                        Util.gridClear(dgListInput_Tray);
                        //dgListInput.ItemsSource = DataTableConverter.Convert(dtSource);
                        Util.GridSetData(dgListInput_Tray, dtSource, FrameOperation, true);
                        txtLotRTDRequest_Tray.Text = string.Empty;
                        txtLotRTDRequest_Tray.Focus();
                    }
                    else
                    {

                        Util.gridClear(dgListInput_Tray);
                        //Util.gridClear(dgSelectInput_Prcess);
                        Util.GridSetData(dgListInput_Tray, dtRslt, FrameOperation, true);
                    }


                }
                else
                {
                    //dgListInput_Tray.ItemsSource = DataTableConverter.Convert(dtRslt);
                    Util.gridClear(dgListInput_Tray);
                    Util.GridSetData(dgListInput_Tray, dtRslt, FrameOperation, true);
                    
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void GetCostNtr_Tray()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("RESNCODE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));


                DataRow dr = dtRqst.NewRow();


                dr["AREAID"] = Util.GetCondition(cboAreaRequest_Tray, bAllNull: true);
                dr["PROCID"] = Procid;
                dr["RESNCODE"] = Util.GetCondition(cboReqCode_Tray, bAllNull: true);
                dr["LANGID"] = LoginInfo.LANGID;


                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QMS_REQ_COST_NTR", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    txtCostCenter_Tray.Text = dtRslt.Rows[0]["COST_CNTR_NAME"].ToString();
                    txtCostCenter_Tray.Tag = dtRslt.Rows[0]["COST_CNTR_ID"].ToString();
                }
                else
                {
                    txtCostCenter_Tray.Text = string.Empty;
                    txtCostCenter_Tray.Tag = string.Empty;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region [QMS 검사의뢰이력]
        public void GetList_QMS_REQ_History()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("FROM_DATE", typeof(String));
                dtRqst.Columns.Add("TO_DATE", typeof(String));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("INSPECREQ", typeof(string));
                dtRqst.Columns.Add("JUDGRESULT", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("TRAYCHECK", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                //ldpDateFromHist.SelectedDateTime.ToShortDateString();
                DataRow dr = dtRqst.NewRow();
                dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                if(cboPalletTray.SelectedValue.ToString() == "SELECT")
                {
                    Util.MessageValidation("SFU4149"); //구분을 선택하세요
                    return ;
                }
                if (cboPalletTray.SelectedValue.ToString() != "TRAY")
                {
                    // N4/5/6 POUCH 인 경우 라인선택이 필수가 아님.
                    if(LoginInfo.SYSID.Equals("GMES-S-N4") || LoginInfo.SYSID.Equals("GMES-S-N5") || LoginInfo.SYSID.Equals("GMES-S-N6"))
                    {
                        dr["PROCID"] = Util.GetCondition(cboProcessHistory, bAllNull: true);
                    } else
                    {
                        dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHistory, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                        if (dr["EQSGID"].Equals("")) return;
                        dr["PROCID"] = Util.GetCondition(cboProcessHistory, bAllNull: true);
                    }
                }
                if (cboPalletTray.SelectedValue.ToString() == "TRAY")
                {
                    dr["TRAYCHECK"] = "Y";
                    dr["AREAID"] = Util.GetCondition(cboAreaHistory, bAllNull: true);
                }
               
                dr["INSPECREQ"] = Util.GetCondition(cboReqHistory, bAllNull: true);
                dr["JUDGRESULT"] = Util.GetCondition(cboJudgResult, bAllNull: true);
                dr["PJT_NAME"] = txtPjtHistory.Text;
                dr["PRODID"] = txtProdID.Text;
                dr["LOTID_RT"] = txtLotHistory.Text;
                dr["LOTID"] = Util.GetCondition(txtPalletHistory);
                dr["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = null;
                if (cboPalletTray.SelectedValue.ToString() == "TRAY")
                {
                    dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QMS_REQ_HISTORY_TRAY", "INDATA", "OUTDATA", dtRqst);
                }
                else
                {
                    dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QMS_REQ_HISTORY", "INDATA", "OUTDATA", dtRqst);
                }

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    Util.gridClear(dgListReqQmsHistory);
                    Util.gridClear(dgSelectInput_History);
                    return;
                }
                Util.gridClear(dgSelectInput_History);
                dgListReqQmsHistory.ItemsSource = DataTableConverter.Convert(dtRslt);
                //Util.GridSetData(dgListReqQmsHistory, dtRslt, FrameOperation, true);
                             
                dgListReqQmsHistory.MergingCells -= DgListReqQmsHistory_MergingCells;
                string[] sColumnName = new string[] { "CHK", "INSP_REQ_ID_2", "INSP_REQ_ID", "LOTID_RT", "PRJT_NAME", "PRODID", "INSP_MED_CLSS_NAME", "INSP_MED_CLSS_CODE", "REQ_QTY", "INSP_PROG_NAME", "INSP_PROG_CODE", "NOTE", "EQSGID", "EQSGNAME", "REQ_USERNAME", "REQ_USERID", "REQ_DTTM", "LOTID", "TRAYID" };
                _Util.SetDataGridMergeExtensionCol(dgListReqQmsHistory, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
               
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void DgListReqQmsHistory_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            
            return;
            //throw new NotImplementedException();
        }

        public void GetLotList_Cancel_Tag(DataTable Result)
        {
            try
            {
                COM001_095_PRINT popupCancelTagPrint = new COM001_095_PRINT();
                popupCancelTagPrint.FrameOperation = this.FrameOperation;

                object[] parameters = new object[3];

                if (Result.Rows.Count > 0)
                {
                    parameters[0] = "C"; //QMS 검사의뢰취소
                    parameters[1] = Result.Rows[0]["PQC_INSP_REQ_ID"].ToString();
                    parameters[2] = Result.Rows[0]["INSP_QTY"].ToString();
                }

                C1WindowExtension.SetParameters(popupCancelTagPrint, parameters);

                //popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);
                popupCancelTagPrint.Closed += new EventHandler(popupCancelTagPrint_Closed);

                grdMain.Children.Add(popupCancelTagPrint);
                popupCancelTagPrint.BringToFront();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }
        private void popupCancelTagPrint_Closed(object sender, EventArgs e)
        {
            COM001_095_PRINT popupCancelTagPrint = new COM001_095_PRINT();
            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            Util.gridClear(dgListReqQmsHistory);
            Util.gridClear(dgSelectInput_History);
            GetList_QMS_REQ_History();

            this.grdMain.Children.Remove(popupCancelTagPrint);
        }

        private void GetDetailLot(string sInsp_Req)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("INSP_REQ_ID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["INSP_REQ_ID"] = sInsp_Req;
                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new DataTable();
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QMS_REQ_HISTORY_DETAIL", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgSelectInput_History, dtRslt, FrameOperation, true);
               
                
                //for (int i = 0; i < dgSelectInput_History.Rows.Count; i++)
                //{
                //    if (cboPalletTray.SelectedValue.ToString() == "TRAY")
                //    {
                //        DataTableConverter.SetValue(dgSelectInput_History.Rows[i].DataItem, "PALLETID", string.Empty);
                //        DataTableConverter.SetValue(dgSelectInput_History.Rows[i].DataItem, "CTNR_ID", string.Empty);
                //    }
                //    else if (cboPalletTray.SelectedValue.ToString() == "CTNR_ID")
                //    {
                //        DataTableConverter.SetValue(dgSelectInput_History.Rows[i].DataItem, "PALLETID", string.Empty);
                //        DataTableConverter.SetValue(dgSelectInput_History.Rows[i].DataItem, "TRAY", string.Empty);
                //    }
                //    else
                //    {
                //        //DataTableConverter.SetValue(dgSelectInput_History.Rows[i].DataItem, "CTNR_ID", string.Empty);
                //        DataTableConverter.SetValue(dgSelectInput_History.Rows[i].DataItem, "TRAY", string.Empty);
                //    }
                //}

                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        

        public void GetLotList_CancelDtail_Tag(DataTable Result)
        {
            try
            {
                COM001_095_PRINT popupCancelTagPrint_Detail = new COM001_095_PRINT();
                popupCancelTagPrint_Detail.FrameOperation = this.FrameOperation;

                object[] parameters = new object[3];

                if (Result.Rows.Count > 0)
                {
                    parameters[0] = "CD"; //QMS 물청취소
                    parameters[1] = Result;
                    parameters[2] = string.Empty;
                }

                C1WindowExtension.SetParameters(popupCancelTagPrint_Detail, parameters);

                //popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);
                popupCancelTagPrint_Detail.Closed += new EventHandler(popupCancelTagPrint_DeTail_Closed);

                grdMain.Children.Add(popupCancelTagPrint_Detail);
                popupCancelTagPrint_Detail.BringToFront();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }
        private void popupCancelTagPrint_DeTail_Closed(object sender, EventArgs e)
        {
            COM001_095_PRINT popupCancelTagPrint_Detail = new COM001_095_PRINT();
            Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            Util.gridClear(dgSelectInput_History);
          
            this.grdMain.Children.Remove(popupCancelTagPrint_Detail);
        }







        #endregion

        #region [QMS 검사의뢰상세이력]
        public void GetList_QMS_REQ_History_Detail()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("FROM_DATE", typeof(String));
                dtRqst.Columns.Add("TO_DATE", typeof(String));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("INSPECREQ", typeof(string));
                dtRqst.Columns.Add("JUDGRESULT", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                //ldpDateFromHist.SelectedDateTime.ToShortDateString();
                DataRow dr = dtRqst.NewRow();
                dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist_Detail);
                dr["TO_DATE"] = Util.GetCondition(ldpDateToHist_Detail);
                if (cboAreaHistory_Detail.SelectedValue.ToString() == "SELECT")
                {
                    Util.MessageValidation("SFU4238"); //동정보를 선택하세요
                    return;
                }
                dr["AREAID"] = Util.GetCondition(cboAreaHistory_Detail, bAllNull: true);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHistory_Detail, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboProcessHistory_Detail, bAllNull: true);
                dr["INSPECREQ"] = Util.GetCondition(cboReqHistory_Detail, bAllNull: true);
                dr["JUDGRESULT"] = Util.GetCondition(cboJudgResult_Detail, bAllNull: true);
                dr["PJT_NAME"] = txtPjtHistory_Detail.Text;
                dr["PRODID"] = txtProdID_Detail.Text;
                dr["LOTID_RT"] = txtLotHistory_Detail.Text;
                dr["LOTID"] = Util.GetCondition(txtPalletHistory_Detail);
                dr["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = null;
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QMS_REQ_HISTORY_DETAIL_ALL", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    Util.gridClear(dgListReqQmsHistory_Detail);
                    return;
                }
                Util.GridSetData(dgListReqQmsHistory_Detail, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #endregion

        #region [QMS 검사의뢰(Pallet)]
        private void QmsRequest()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("INSP_MED_CLSS_CODE", typeof(string));
            inDataTable.Columns.Add("LOGISTICS_FLAG", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("RESNFLAG", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("COST_CNTR_ID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["INSP_MED_CLSS_CODE"] = cboInspectReq.SelectedValue.ToString(); //의뢰구분
            row["LOGISTICS_FLAG"] = "P"; //물류단위 T:Tray, P:PALLET
            row["PROD_LOTID"] = string.Empty;
            row["RESNFLAG"] ="Y";
            row["RESNCODE"] = cboReqCode.SelectedValue.ToString();
            row["COST_CNTR_ID"] = txtCostCenter.Tag.ToString();
            row["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "PROCID"));
            row["EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "EQSGID"));
            row["NOTE"] = txtReson.Text;
            row["USERID"] = txtUserNameCr.Tag.ToString();

            inDataTable.Rows.Add(row);

            //제품의뢰
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("INPUT_LOTID", typeof(string));
            inLot.Columns.Add("WIPQTY", typeof(Decimal)); //검사의뢰 수량

            for (int i = 0; i < dgSelectInput.Rows.Count; i++)
            {
       
                row = inLot.NewRow();
                row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "PALLETID"));
                row["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "INSPQTY"));
                inLot.Rows.Add(row);
               
            }


            try
            {
                //제품의뢰 처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_REQUEST_PQC", "INDATA,INLOT", "OUTDATA", (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    GetLotList_Tag(Result.Tables["OUTDATA"]);
              

                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_REQUEST_PQC", ex.Message, ex.ToString());

            }
        }
        #endregion


        #region [QMS 검사의뢰(대차)]


        #endregion
        
        #region [QMS 검사의뢰(Tray)]
        private void QmsRequest_Tray()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("INSP_MED_CLSS_CODE", typeof(string));
            inDataTable.Columns.Add("LOGISTICS_FLAG", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("ASSY_LOTID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("RESNFLAG", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("COST_CNTR_ID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["INSP_MED_CLSS_CODE"] = cboInspectReq_Tray.SelectedValue.ToString(); //의뢰구분
            row["LOGISTICS_FLAG"] = "T"; //물류단위 T:Tray, P:PALLET
            row["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput_Tray.Rows[0].DataItem, "LOTID")).Substring(0, 8);
            row["ASSY_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput_Tray.Rows[0].DataItem, "LOTID"));
            row["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput_Tray.Rows[0].DataItem, "TRAYID"));
            row["RESNFLAG"] = "Y";
            row["RESNCODE"] = cboReqCode_Tray.SelectedValue.ToString();
            row["COST_CNTR_ID"] = txtCostCenter_Tray.Tag.ToString();
            //if (MCC_MCS == "MCS") //초소형
            //{
            //    row["PROCID"] = "F5100";
            //}
            //else
            //{
            //    row["PROCID"] = "F5000";
            //}
            row["PROCID"] = Procid;
            row["EQSGID"] = cboEquipmentSegmentRequest_Tray.SelectedValue.ToString();
            row["NOTE"] = txtReson_Tray.Text;
            row["USERID"] = txtUserName_Tray.Tag.ToString();

            inDataTable.Rows.Add(row);

            //제품의뢰
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("INPUT_LOTID", typeof(string));
            inLot.Columns.Add("WIPQTY", typeof(Decimal)); //검사의뢰 수량
          
            row = inLot.NewRow();
            row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput_Tray.Rows[0].DataItem, "LOTID")).Substring(0,8);
            row["WIPQTY"] = txtInspQty_Tray.Text;
            inLot.Rows.Add(row);



            try
            {
                //제품의뢰 처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_REQUEST_PQC", "INDATA,INLOT", "OUTDATA", (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                    Util.gridClear(dgListInput_Tray);
                    Util.gridClear(dgSelectInput_Tray);
                    cboInspectReq_Tray.SelectedIndex = 0;
                    txtUserName_Tray.Text = LoginInfo.USERNAME;
                    txtUserName_Tray.Tag = LoginInfo.USERID;
                    txtInspQty_Tray.Text = string.Empty;
                    txtReson_Tray.Text = string.Empty;
                    txtCostCenter_Tray.Text = string.Empty;
                    cboReqCode_Tray.SelectedIndex = 0;
                    //GetLotList_Tray(true);

                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_REQUEST_PQC", ex.Message, ex.ToString());

            }
        }
        #endregion

        #region [QMS 검사의뢰 이력]
        private void Cancel_QmsRequest()
        {
            DataSet inData = new DataSet();
            
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("PQC_INSP_REQ_ID", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("INSP_QTY", typeof(string));
            DataRow row = null;
            for (int i = 0; i < dgListReqQmsHistory.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgListReqQmsHistory.Rows[i].DataItem, "CHK")) == "True")
                {
                    row = inDataTable.NewRow();
                    row["SRCTYPE"] = "UI";
                    row["IFMODE"] = "OFF";
                    row["PQC_INSP_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgListReqQmsHistory.Rows[i].DataItem, "INSP_REQ_ID"));
                    row["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgListReqQmsHistory.Rows[i].DataItem, "INSP_MED_CLSS_CODE"));
                    row["USERID"] = LoginInfo.USERID;
                    row["INSP_QTY"] = Util.NVC(DataTableConverter.GetValue(dgListReqQmsHistory.Rows[i].DataItem, "REQ_QTY"));
                    inDataTable.Rows.Add(row);
                }
            }
            try
            {
                //제품의뢰 처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_PQC", "INDATA", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                   
                    if (cboPalletTray.SelectedValue.ToString() == "TRAY")
                    {
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        Util.gridClear(dgListReqQmsHistory);
                        Util.gridClear(dgSelectInput_History);
                        GetList_QMS_REQ_History();
                    }
                    else
                    {
                        GetLotList_Cancel_Tag(inData.Tables[0]);
                    }

                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_CANCEL_PQC", ex.Message, ex.ToString());

            }
        }

        //대차별 취소
        private void Cancel_QmsRequest_Ctnr()
        {
            DataSet inData = new DataSet();



            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("ACT_USERID", typeof(string));
            DataRow row = null;
            for (int i = 0; i < dgListReqQmsHistory.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgListReqQmsHistory.Rows[i].DataItem, "CHK")) == "True")
                {
                    row = inDataTable.NewRow();
                    row["SRCTYPE"] = "UI";
                    row["IFMODE"] = "OFF";
                    row["AREAID"] = Util.NVC(DataTableConverter.GetValue(dgListReqQmsHistory.Rows[i].DataItem, "AREAID"));
                    row["USERID"] = LoginInfo.USERID;
                    row["ACT_USERID"] = txtUserNameCr_History.Tag.ToString();
                    inDataTable.Rows.Add(row);
                }
            }

            DataTable innsp = inData.Tables.Add("ININSP");
            innsp.Columns.Add("PQC_INSP_REQ_ID", typeof(string));
            for (int i = 0; i < dgListReqQmsHistory.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgListReqQmsHistory.Rows[i].DataItem, "CHK")) == "True")
                {
                    row = innsp.NewRow();
                    row["PQC_INSP_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgListReqQmsHistory.Rows[i].DataItem, "INSP_REQ_ID"));
                    innsp.Rows.Add(row);
                }
            }
  
            try
            {
                //제품의뢰 처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_QMS_INSP_CTNR", "INDATA,ININSP", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                   
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        Util.gridClear(dgListReqQmsHistory);
                        Util.gridClear(dgSelectInput_History);
                        GetList_QMS_REQ_History();
                   

                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_CANCEL_QMS_INSP_CTNR", ex.Message, ex.ToString());

            }
        }

        private void Set_Combo_Pallet_Tray(C1ComboBox cbo)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn() { ColumnName = "CBO_CODE", DataType = typeof(string) });
                dt.Columns.Add(new DataColumn() { ColumnName = "CBO_NAME", DataType = typeof(string) });

                DataRow row = dt.NewRow();

                row = dt.NewRow();
                row["CBO_CODE"] = "SELECT";
                row["CBO_NAME"] = "SELECT";
                dt.Rows.Add(row);

                row = dt.NewRow();
                row["CBO_CODE"] = "PALLET";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("팔레트");
                dt.Rows.Add(row);

                row = dt.NewRow();
                row["CBO_CODE"] = "CTNR";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("대차");
                dt.Rows.Add(row);

                row = dt.NewRow();
                row["CBO_CODE"] = "TRAY";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("트레이"); 
                dt.Rows.Add(row);

                cbo.ItemsSource = DataTableConverter.Convert(dt);

                cbo.SelectedIndex = 0;
              
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                Logger.Instance.WriteLine(Logger.OPERATION_R + "SetTimeCombo", ex);
            }
        }


        private void Detail_Cancel_QmsRequest()
        {

            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("PQC_INSP_REQ_ID", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
           
            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["PQC_INSP_REQ_ID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput_History.Rows[0].DataItem, "INSP_REQ_ID"));
            row["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput_History.Rows[0].DataItem, "INSP_MED_CLSS_CODE"));
            row["USERID"] = LoginInfo.USERID;
          
            inDataTable.Rows.Add(row);

            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("INSP_REQ_SEQNO", typeof(string));
          
            for (int i = 0; i < dgSelectInput_History.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSelectInput_History.Rows[i].DataItem, "CHK")) == "1")
                {
                    row = inLot.NewRow();
                    row["INSP_REQ_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput_History.Rows[i].DataItem, "INSP_REQ_SEQNO"));
                    inLot.Rows.Add(row);
                }
            }
            try
            {
                //물청취소
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_PQC_DETL", "INDATA,INLOT", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    if (cboPalletTray.SelectedValue.ToString() == "TRAY")
                    {
                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        Util.gridClear(dgSelectInput_History);
                    }
                    else
                    {
                        //GetLotList_CancelDtail_Tag(inData.Tables[1]);
                        GetLotList_CancelDtail_Tag(inData.Tables["INLOT"]);
                        //GetLotList_Tag(Result.Tables["OUTDATA"]);
                    }

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_CANCEL_PQC_DETL", ex.Message, ex.ToString());
            }
         
        }

        #endregion

        #region[공통]

        private void GetUserWindow(string Check)
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
               
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                if (Check == "P")
                {
                    Parameters[0] = txtUserNameCr.Text;
                    wndPerson.Closed += new EventHandler(wndUser_Closed);
                }
                else
                {
                    Parameters[0] = txtUserName_Tray.Text;
                    wndPerson.Closed += new EventHandler(wndUser_Closed_Tray);
                }
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUserNameCr.Text = wndPerson.USERNAME;
                txtUserNameCr.Tag = wndPerson.USERID;

            }
        }

        private void GetUserWindow_Tray()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(wndPerson, Parameters);
                Parameters[0] = txtUserName_Tray.Text;
                wndPerson.Closed += new EventHandler(wndUser_Closed_Tray);
       
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        private void wndUser_Closed_Tray(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUserName_Tray.Text = wndPerson.USERNAME;
                txtUserName_Tray.Tag = wndPerson.USERID;

            }
        }



        private void GetUserWindow_TraySelect()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(wndPerson, Parameters);
                Parameters[0] = txtUserName_TraySelect.Text;
                wndPerson.Closed += new EventHandler(wndUser_Closed_TraySelect);

                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        private void wndUser_Closed_TraySelect(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUserName_TraySelect.Text = wndPerson.USERNAME;
                txtUserName_TraySelect.Tag = wndPerson.USERID;

            }
        }

        private void GetUserWindow_Ctnr()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(wndPerson, Parameters);
                Parameters[0] = txtUserNameCr_Ctnr.Text;
                wndPerson.Closed += new EventHandler(wndUser_Closed_Ctnr);

                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        private void wndUser_Closed_Ctnr(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUserNameCr_Ctnr.Text = wndPerson.USERNAME;
                txtUserNameCr_Ctnr.Tag = wndPerson.USERID;

            }
        }

    
        //의뢰취소자
        private void GetUserWindow_History()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(wndPerson, Parameters);
                Parameters[0] = txtUserNameCr_History.Text;
                wndPerson.Closed += new EventHandler(wndUser_Closed_History);

                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }
        //의뢰 취소자 팝업닫기
        private void wndUser_Closed_History(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUserNameCr_History.Text = wndPerson.USERNAME;
                txtUserNameCr_History.Tag = wndPerson.USERID;

            }
        }

        #endregion


        #endregion

        #region [Validation]

        #endregion

        #region [QMS 검사의뢰(Pallet)]
        private bool Validation()
        {

            if(cboInspectReq.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU4150");  //의뢰구분을 선택하세요
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtUserNameCr.Text) || txtUserNameCr.Tag == null)
            {
                Util.MessageValidation("SFU4151");//의뢰자를 선택하세요
                return false;
            }

            //if(chkReq.IsChecked == true)
            //{
            if (cboReqCode.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU4152");//물품청구코드를 선택하세요
                return false;
            }

            if (txtCostCenter.Tag == null)
            {
                Util.MessageValidation("SFU4153");//코스트센터 정보가 없습니다.
                return false;
            }

            if (String.IsNullOrEmpty(txtCostCenter.Tag.ToString()) == true)
            {
                Util.MessageValidation("SFU4153");//코스트센터 정보가 없습니다.
                return false;
            }

            //}

            if (dgSelectInput.Rows.Count==0)
            {
                Util.MessageValidation("SFU4154");//의뢰할 데이터가 없습니다.
                return false;
            }
            for (int i=0; i< dgSelectInput.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "INSPQTY")) == string.Empty || Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "INSPQTY")) =="0")
                {
                    Util.MessageValidation("SFU4155");//검사수량을 등록하세요"
                    return false;
                }

                if (Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "WIPQTY")).Replace(",", "")) < Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "INSPQTY"))))
                {
                    Util.MessageValidation("SFU4156");//검사수량은 재공수량보다 클 수 없습니다.
                    return false;
                }
            }
            if (cboInspectReq.SelectedValue.ToString() == "PQCM001" && Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[0].DataItem, "QMSYN")) == "N")
            {
                Util.MessageValidation("SFU4157");//장기재고검사만 할 수 있는 데이터 입니다.
                return false;
            }
               return true;
        }
        #endregion
        #region [QMS 검사의뢰(대차)]
        private bool Validation_ctnr()
        {

            if (cboInspectReq_Ctnr.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU4150");  //의뢰구분을 선택하세요
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtUserNameCr_Ctnr.Text) || txtUserNameCr_Ctnr.Tag == null)
            {
                Util.MessageValidation("SFU4151");//의뢰자를 선택하세요
                return false;
            }

            //if(chkReq.IsChecked == true)
            //{
            if (cboReqCode_Ctnr.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU4152");//물품청구코드를 선택하세요
                return false;
            }

            if (txtCostCenter_Ctnr.Tag == null)
            {
                Util.MessageValidation("SFU4153");//코스트센터 정보가 없습니다.
                return false;
            }

            if (String.IsNullOrEmpty(txtCostCenter_Ctnr.Tag.ToString()) == true)
            {
                Util.MessageValidation("SFU4153");//코스트센터 정보가 없습니다.
                return false;
            }

            //}

            if (dgSelectInput_Ctnr.Rows.Count == 0)
            {
                Util.MessageValidation("SFU4154");//의뢰할 데이터가 없습니다.
                return false;
            }
                      


            return true;
        }
        #endregion
        #region [QMS 검사의뢰(Tray)]
        private bool Validation_Tray()
        {


            if (dgSelectInput_Tray.Rows.Count == 0)
            {
                Util.MessageValidation("SFU4154");//의뢰할 데이터가 없습니다.
                return false;
            }

            if (cboInspectReq_Tray.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU4150"); //의뢰구분을 선택하세요
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtUserName_Tray.Text) || txtUserName_Tray.Tag == null)
            {
                Util.MessageValidation("SFU4151");//의뢰자를 선택하세요
                return false;
            }

            if(cboEquipmentSegmentRequest_Tray.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU1223");//라인을 선택하세요

                return false;
            }
            
            if (cboReqCode_Tray.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU4152");//물품청구코드를 선택하세요
                return false;
            }
            
            if (txtCostCenter_Tray.Tag == null)
            {
                Util.MessageValidation("SFU4153");//코스트센터 정보가 없습니다.
                return false;
            }
            if (String.IsNullOrEmpty(txtCostCenter_Tray.Tag.ToString()) == true)
            {
                Util.MessageValidation("SFU4153");//코스트센터 정보가 없습니다.
                return false;
            }

            if (txtInspQty_Tray.Text == string.Empty || txtInspQty_Tray.Text == "0")
            {
                Util.MessageValidation("SFU4158");//검사수량을 입력하세요.
                return false;
            }

           


            if (cboInspectReq_Tray.SelectedValue.ToString() == "PQCM001" && Util.NVC(DataTableConverter.GetValue(dgSelectInput_Tray.Rows[0].DataItem, "QMSYN")) == "N")
            {
                Util.MessageValidation("SFU4157");//장기재고검사만 할 수 있는 데이터 입니다.
                return false;
            }

            return true;
        }



        private string PROCID_CHK(string ProdID)
        {
            string ReturnValue = string.Empty;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["PRODID"] = ProdID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCID_CHK", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    ReturnValue = dtRslt.Rows[0]["PROCID"].ToString();
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }


            return ReturnValue;

        }



        #endregion

        #region [QMS 검사의뢰취소]
        private bool CanCel_Validation()
        {

            if (dgListReqQmsHistory.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1905"); //조회된 데이터가 없습니다.
                return false;
            }

            DataRow[] drSelect = DataTableConverter.Convert(dgListReqQmsHistory.ItemsSource).Select("CHK = 'True'");
           
            int CheckCount = 0;

            for (int i = 0; i < dgListReqQmsHistory.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgListReqQmsHistory.Rows[i].DataItem, "CHK")) == "True")
                {
                    CheckCount = CheckCount + 1;
                }
            }

            if (CheckCount  == 0)
            {
                Util.MessageValidation("SFU1275");//선택된 항목이 없습니다.
                return false;
            }
            if (CheckCount > 1)
            {
                Util.MessageValidation("SFU4159");//한건만 선택하세요.
                return false;
            }

            foreach (DataRow dr in drSelect)
            {
                if (dr["INSP_PROG_CODE"].ToString() != "REQUEST")
                {
                    Util.MessageValidation("SFU4160");//"판정결과가 요청상태인 데이터만 취소가능합니다.
                    return false;
                }
            }
            if (cboPalletTray.SelectedValue.ToString() == "CTNR")
            {
                if (string.IsNullOrWhiteSpace(txtUserNameCr_History.Text) || txtUserNameCr_History.Tag == null)
                {
                    Util.MessageValidation("SFU4624");//의뢰취소자를 선택하세요
                    return false;
                }

            }

            return true;
        }


        private bool Detail_CanCel_Validation()
        {

            DataRow[] drSelect = DataTableConverter.Convert(dgSelectInput_History.ItemsSource).Select("CHK = 1");
            if (dgSelectInput_History.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1905"); // 조회된 데이터가 없습니다
                return false;
            }
            if (drSelect.Length == 0)
            {
                Util.MessageValidation("SFU1651");//선택된 항목이 없습니다.
                return false;
            }
            foreach (DataRow drDetail in drSelect)
            {
                if(drDetail["INSP_REQ_PROG_CODE"].ToString() != "REQUEST")
                {
                    Util.MessageValidation("SFU4161");//이미 물청취소한 데이터 입니다.
                    return false;
                }
             
            }
            int CheckCount = 0; //요청상태의 데이터 갯수
            int UnCheckCount = 0; //체크하고 남은 데이터의갯수
            for (int i = 0; i < dgSelectInput_History.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSelectInput_History.Rows[i].DataItem, "INSP_REQ_PROG_CODE")) == "REQUEST")
                {
                    CheckCount = CheckCount + 1;
                }
                if (Util.NVC(DataTableConverter.GetValue(dgSelectInput_History.Rows[i].DataItem, "CHK")) == "0" && Util.NVC(DataTableConverter.GetValue(dgSelectInput_History.Rows[i].DataItem, "INSP_REQ_PROG_CODE")) == "REQUEST")
                {
                    UnCheckCount = UnCheckCount + 1;
                }
            }

            if (dgSelectInput_History.Rows.Count == 1 && Util.NVC(DataTableConverter.GetValue(dgSelectInput_History.Rows[0].DataItem, "INSP_PROG_CODE")) != "RJCT")
            {
                if (CheckCount == 1) //요청상태의 데이터 갯수
                {
                    Util.MessageValidation("SFU4162");//검사요청 상태의 의뢰건수는 1건 이상 남아 있어야 합니다.\n 전체 취소를 위해서는 의뢰취소 처리 바랍니다.
                    return false;
                }
                if (UnCheckCount == 0)//체크하고 남은 데이터의갯수
                {
                    Util.MessageValidation("SFU4162");//검사요청 상태의 의뢰건수는 1건 이상 남아 있어야 합니다.\n 전체 취소를 위해서는 의뢰취소 처리 바랍니다.
                    return false;
                }
            }

            return true;
        }













        #endregion

        
        //대차별 QMS 의뢰
        private void btnQmsRequest_Ctnr_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_ctnr())
                {
                    return;
                }
                //병합 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4140"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                QmsRequest_Ctnr();
                            }
                        });
                //검사의뢰 하시겠습니까?
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //Qms 검사의뢰 (대차)
        private void QmsRequest_Ctnr()
        {
            DataSet inData = new DataSet();
            //마스터 정보




            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("INSP_MED_CLSS_CODE", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("COST_CNTR_ID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("ACT_USERID", typeof(string));
        
            DataRow row = null;

            row = inDataTable.NewRow();
            row["INSP_MED_CLSS_CODE"] = cboInspectReq_Ctnr.SelectedValue.ToString(); //의뢰구분
            row["RESNCODE"] = cboReqCode_Ctnr.SelectedValue.ToString();
            row["COST_CNTR_ID"] = txtCostCenter_Ctnr.Tag.ToString();
            row["AREAID"] = LoginInfo.CFG_AREA_ID;
            row["EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput_Ctnr.Rows[0].DataItem, "EQSGID"));
            row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput_Ctnr.Rows[0].DataItem, "PRODID"));
            row["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput_Ctnr.Rows[0].DataItem, "PROCID"));
            row["USERID"] = LoginInfo.USERID;
            row["NOTE"] = txtReson_Ctnr.Text;
            row["ACT_USERID"] = txtUserNameCr.Tag.ToString();

            inDataTable.Rows.Add(row);

            //제품의뢰
            DataTable inCtnr = inData.Tables.Add("INCTNR");
            inCtnr.Columns.Add("CTNR_ID", typeof(string));
           
            for (int i = 0; i < dgSelectInput_Ctnr.Rows.Count; i++)
            {

                row = inCtnr.NewRow();
                row["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput_Ctnr.Rows[i].DataItem, "CTNR_ID"));

                inCtnr.Rows.Add(row);

            }


            try
            {
                //제품의뢰 처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_QMS_INSP_REQ_CTNR", "INDATA,INCTNR", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    Util.gridClear(dgListInput_Ctnr);
                    Util.gridClear(dgListInput_Ctnr);
                    cboInspectReq_Ctnr.SelectedIndex = 0;
                    txtUserNameCr_Ctnr.Text = LoginInfo.USERNAME;
                    txtUserNameCr_Ctnr.Tag = LoginInfo.USERID;
                    txtInspQty_Ctnr.Text = string.Empty;
                    txtReson_Ctnr.Text = string.Empty;
                    txtCostCenter_Ctnr.Text = string.Empty;
                    cboReqCode_Ctnr.SelectedIndex = 0;
                    GetLotList_Ctnr(true);


                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_QMS_INSP_REQ_CTNR", ex.Message, ex.ToString());

            }
        }

        private void dgLotTraySelect_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //(grdResult.Children[0] as UcAssyResult).dtpCaldate.SelectedDataTimeChanged -= OndtpCaldate_SelectedDataTimeChanged;
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }
                        //row 색 바꾸기
                        //dgListInput_Tray.SelectedIndex = idx;

                        string sLotid = Util.NVC(DataTableConverter.GetValue(dgListInput_TraySelect.Rows[idx].DataItem, "LOTID"));
                        string sAreaid = Util.NVC(DataTableConverter.GetValue(dgListInput_TraySelect.Rows[idx].DataItem, "AREAID"));
                        string sEqsgid = Util.NVC(DataTableConverter.GetValue(dgListInput_TraySelect.Rows[idx].DataItem, "EQSGID"));

                        // Tray detail 정보 조회
                        GetLotList_TraySelect_detail(sLotid, sEqsgid);

                        //공정
                        Procid = PROCID_CHK(Util.NVC(DataTableConverter.GetValue(dgListInput_TraySelect.Rows[idx].DataItem, "PRODID")));
                        CommonCombo _combo = new CommonCombo();
                        String[] sFilterQmsRequest = { sAreaid, Procid };
                        _combo.SetCombo(cboReqCode_TraySelect, CommonCombo.ComboStatus.SELECT, sFilter: sFilterQmsRequest, sCase: "REQ_CODE");

                        //String[] sFilterQmsRequest = { cboAreaRequest_Tray.SelectedValue.ToString(), Procid };
                        //_combo.SetCombo(cboReqCode_Tray, CommonCombo.ComboStatus.SELECT, sFilter: sFilterQmsRequest, sCase: "REQ_CODE");

                        if (cboReqCode_TraySelect.Items.Count == 2)
                        {
                            cboReqCode_TraySelect.SelectedIndex = 1;
                            GetCostNtr_TraySelect();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// [C20190613_17057] 입력값 초기화 기능
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReSetTray_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListInput_TraySelect);
            Util.gridClear(dgSelectInput_TraySelect);
            cboInspectReq_TraySelect.SelectedIndex = 0;
            txtUserName_TraySelect.Text = LoginInfo.USERNAME;
            txtUserName_TraySelect.Tag = LoginInfo.USERID;
            txtReson_TraySelect.Text = string.Empty;
            txtCostCenter_TraySelect.Text = string.Empty;
            cboReqCode_TraySelect.SelectedIndex = 0;

        }

        /// <summary>
        /// [C20190613_17057] 검사의뢰 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnQmsRequest_TraySelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_TraySelect())
                {
                    return;
                }
                //검사의뢰하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4140"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                QmsRequest_TraySelect();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        ///  [C20190613_17057] 저장 전 입력값 확인
        /// </summary>
        /// <returns></returns>
        private bool Validation_TraySelect()
        {

            // 수정 대상

            if (dgSelectInput_TraySelect.Rows.Count == 0)
            {
                Util.MessageValidation("SFU4154");//의뢰할 데이터가 없습니다.
                return false;
            }

            if (cboInspectReq_TraySelect.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU4150"); //의뢰구분을 선택하세요
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUserName_TraySelect.Text) || txtUserName_TraySelect.Tag == null)
            {
                Util.MessageValidation("SFU4151");//의뢰자를 선택하세요
                return false;
            }

            if (cboEquipmentSegmentTray.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU1223");//라인을 선택하세요

                return false;
            }

            if (cboReqCode_TraySelect.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU4152");//물품청구코드를 선택하세요
                return false;
            }

            if (txtCostCenter_TraySelect.Tag == null)
            {
                Util.MessageValidation("SFU4153");//코스트센터 정보가 없습니다.
                return false;
            }

            if (String.IsNullOrEmpty(txtCostCenter_TraySelect.Tag.ToString()) == true)
            {
                Util.MessageValidation("SFU4153");//코스트센터 정보가 없습니다.
                return false;
            }

            if (txtInspQty_TraySelect.Text == string.Empty || txtInspQty_TraySelect.Text == "0")
            {
                Util.MessageValidation("SFU4158");//검사수량을 입력하세요.
                return false;
            }


            if (cboInspectReq_TraySelect.SelectedValue.ToString() == "PQCM001" && Util.NVC(DataTableConverter.GetValue(dgSelectInput_TraySelect.Rows[0].DataItem, "QMSYN")) == "N")
            {
                Util.MessageValidation("SFU4157");//장기재고검사만 할 수 있는 데이터 입니다.
                return false;
            }

            return true;
        }

        #region [QMS 검사의뢰(TraySelect)]
        /// <summary>
        /// [C20190613_17057] 검사의뢰 이벤트 처리
        /// </summary>
        private void QmsRequest_TraySelect()
        {

            //
            int iRow = -1;
            for (int i = 0; i < dgSelectInput_TraySelect.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSelectInput_TraySelect.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgSelectInput_TraySelect.Rows[i].DataItem, "CHK")).Equals("True"))
                {
                    iRow = i;
                    break;
                }
            }

            if (iRow < 0)
            {
                Util.MessageValidation("SFU8145");//검사의뢰 대상 Tray가 없습니다.
                return;
            }
            // 로직 확인 후 수정 대상
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("INSP_MED_CLSS_CODE", typeof(string));
            inDataTable.Columns.Add("LOGISTICS_FLAG", typeof(string));
            inDataTable.Columns.Add("PROD_LOTID", typeof(string));
            inDataTable.Columns.Add("ASSY_LOTID", typeof(string));
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("RESNFLAG", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("COST_CNTR_ID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQSGID", typeof(string));
            inDataTable.Columns.Add("NOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["INSP_MED_CLSS_CODE"] = cboInspectReq_TraySelect.SelectedValue.ToString(); //의뢰구분
            row["LOGISTICS_FLAG"] = "T"; //물류단위 T:Tray, P:PALLET
            row["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput_TraySelect.Rows[iRow].DataItem, "LOTID")).Substring(0, 8);
            row["ASSY_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput_TraySelect.Rows[iRow].DataItem, "LOTID")).Substring(0, 10);
            row["CSTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput_TraySelect.Rows[iRow].DataItem, "TRAYID"));
            row["RESNFLAG"] = "Y";
            row["RESNCODE"] = cboReqCode_TraySelect.SelectedValue.ToString();
            row["COST_CNTR_ID"] = txtCostCenter_TraySelect.Tag.ToString();
            //if (MCC_MCS == "MCS") //초소형
            //{
            //    row["PROCID"] = "F5100";
            //}
            //else
            //{
            //    row["PROCID"] = "F5000";
            //}
            row["PROCID"] = Procid;

            //C20190613_17057 라인선택 조회 조건이 없어 동으로 라인 설정 처리(향후 변경 대상)
            if (LoginInfo.CFG_AREA_ID.Equals("M1"))
                row["EQSGID"] = "M1CF1";
            else if (LoginInfo.CFG_AREA_ID.Equals("M2"))
                row["EQSGID"] = "M2CF3";

            //row["EQSGID"] = cboEquipmentSegmentTray.SelectedValue.ToString(); //????

            row["NOTE"] = txtReson_TraySelect.Text;
            row["USERID"] = txtUserName_TraySelect.Tag.ToString();

            inDataTable.Rows.Add(row);

            //제품의뢰
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("INPUT_LOTID", typeof(string));
            inLot.Columns.Add("WIPQTY", typeof(Decimal)); //검사의뢰 수량

            row = inLot.NewRow();
            row["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput_TraySelect.Rows[iRow].DataItem, "LOTID")).Substring(0, 8);
            row["WIPQTY"] = txtInspQty_TraySelect.Text;
            inLot.Rows.Add(row);



            try
            {
                //제품의뢰 처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_REQUEST_PQC", "INDATA,INLOT", "OUTDATA", (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                    Util.gridClear(dgListInput_TraySelect);
                    Util.gridClear(dgSelectInput_TraySelect);
                    cboInspectReq_TraySelect.SelectedIndex = 0;
                    txtUserName_TraySelect.Text = LoginInfo.USERNAME;
                    txtUserName_TraySelect.Tag = LoginInfo.USERID;
                    txtInspQty_TraySelect.Text = string.Empty;
                    txtReson_TraySelect.Text = string.Empty;
                    txtCostCenter_TraySelect.Text = string.Empty;
                    cboReqCode_TraySelect.SelectedIndex = 0;
                    //GetLotList_Tray(true);

                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_REQUEST_PQC", ex.Message, ex.ToString());

            }
        }
        #endregion

        /// <summary>
        /// [C20190613_17057] 검사의뢰자 ID 확인
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtUserName_TraySelect_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow_TraySelect();
            }
        }

        /// <summary>
        /// [C20190613_17057] 검사의뢰자 ID 확인
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUser_TraySelect_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow_TraySelect();
        }

        /// <summary>
        /// [C20190613_17057] 조회 이벤트 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchTray_Click(object sender, RoutedEventArgs e)
        {
            // 수정 
            // 입력값 체크 기능 추가 - 검색 조건이 기간 이외 하나도 없을 경우 메시지 처리

            GetLotList_TraySelect();
        }

        /// <summary>
        /// [C20190613_17057] 검사의수량 확인 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtInspQty_TraySelect_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtInspQty_TraySelect.Text, 0))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// [C20190613_17057] 조립LOT 조회 후 이벤트 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgListInput_TraySelect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {

                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    //if (e.Cell.Column.Name == "QMSYN")
                    //{
                    //    if (Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["QMSYN"].Index).Value) == "N")
                    //    {
                    //        DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "QMSYN", ObjectDic.Instance.GetObjectName("의뢰불가능"));

                    //    }
                    //    else if (Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["QMSYN"].Index).Value) == "Y")
                    //    {

                    //        DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "QMSYN", ObjectDic.Instance.GetObjectName("의뢰가능"));
                    //    }

                    //}
                    //else
                    //{
                    //    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    //}

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// [C20190613_17057] 물품 청구 구분 선택 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboReqCode_TraySelect_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // 수정 대상
            try
            {
                if (cboReqCode_TraySelect.SelectedValue != null)
                {
                    GetCostNtr_TraySelect();
                }
                else
                {
                    txtCostCenter_TraySelect.Text = string.Empty;
                    txtCostCenter_TraySelect.Tag = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// [C20190613_17057] Tray 선택 대상 List 조회 후 이벤트 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgSelectInput_TraySelect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name.Equals("TRAYID"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                    }
                }

            }));
        }


        #region QMS 검사의뢰(Tray)
        /// <summary>
        /// [C20190613_17057] 검색 조건에 맞는 데이터 조회 함수
        /// </summary>
        /// <param name="bButton"></param>
        public void GetLotList_TraySelect()
        {
            // 수정 대상 (전체)
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                //dtRqst.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                // 수정 대상 - 입력값 체크 추가 필요성 확인 후 처리 (ex)제품코드,pjt코드,lotid 값중 하나이상 입력이 필요함
                //if (Util.GetCondition(txtLotRTDRequest_Tray).Equals(""))
                //{
                //    Util.AlertInfo("SFU4147"); //조립LOTID를 입력하세요
                //    return;
                //}
                //if (txtLotRTDRequest_Tray.Text.Length < 8)
                //{
                //    Util.AlertInfo("SFU4148"); //조립LOTID는 8자리 이상입니다.
                //    return;
                //}


                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtLotRTDTray.Text;
                dr["PRJT_NAME"] = txtPrjtNameTray.Text;
                dr["PRODID"] = txtProdidTray.Text;
                dr["FROM_DATE"] = Util.GetCondition(ldpDateFromTray);
                dr["TO_DATE"] = Util.GetCondition(ldpDateToTray);
                dr["AREAID"] = Util.GetCondition(cboAreaTray, bAllNull: true);
                //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentTray, "SFU1223"); //조회 조건에서 제외 함 (저장할때 사용하는 기능임)

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QMS_REQ_LOT_TRAY_OPT", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt != null &&  dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(dgListInput_TraySelect, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectInput_TraySelect);
                }
                else
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListInput_TraySelect, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectInput_TraySelect);
                    return;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// [C20190613_17057] 물품 청구 선택 후 이벤트 처리
        /// </summary>
        private void GetCostNtr_TraySelect()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("RESNCODE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));


                DataRow dr = dtRqst.NewRow();


                dr["AREAID"] = Util.GetCondition(cboAreaRequest_Tray, bAllNull: true);
                dr["PROCID"] = Procid;
                dr["RESNCODE"] = Util.GetCondition(cboReqCode_TraySelect, bAllNull: true);
                dr["LANGID"] = LoginInfo.LANGID;


                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QMS_REQ_COST_NTR", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    txtCostCenter_TraySelect.Text = dtRslt.Rows[0]["COST_CNTR_NAME"].ToString();
                    txtCostCenter_TraySelect.Tag = dtRslt.Rows[0]["COST_CNTR_ID"].ToString();
                }
                else
                {
                    txtCostCenter_TraySelect.Text = string.Empty;
                    txtCostCenter_TraySelect.Tag = string.Empty;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetLotList_TraySelect_detail(string sLotid, string sEqsgid)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LOTID"] = sLotid;
                dr["EQSGID"] = sEqsgid;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QMS_REQ_LOT_TRAY_OPT_DETAIL", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(dgSelectInput_TraySelect, dtRslt, FrameOperation, true);
                }
                else
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        private void dgInputTraySelect_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
            }
        }


    }
}
