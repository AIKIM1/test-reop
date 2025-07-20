/*************************************************************************************
 Created Date : 2017.08.23
      Creator : 오화백
   Decription : 활성화 펠리트 폐기/취소
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.23  오화백 : Initial Created.





 
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
    public partial class COM001_100 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
       
        string Procid = string.Empty;
        string Procid_Ncr = string.Empty;
        public COM001_100()
        {
            InitializeComponent();
            InitCombo();
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
         
            #region 팔레트 폐기  
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegmentScrap };
            _combo.SetCombo(cboAreaScrap, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaChild);
            
            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboAreaScrap };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcessScrap };
            _combo.SetCombo(cboEquipmentSegmentScrap, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            
            //공정
            C1ComboBox[] cbProcessParent = { cboEquipmentSegmentScrap };
            String[] cbProcessChild = { "SEARCH", string.Empty, string.Empty, string.Empty };
            _combo.SetCombo(cboProcessScrap, CommonCombo.ComboStatus.ALL, sFilter: cbProcessChild, sCase: "PROCESS_SORT", cbParent: cbProcessParent);


            //폐기사유코드
            string[] sFilter = { "SCRAP_LOT" };
            _combo.SetCombo(cboActivitiReason, CommonCombo.ComboStatus.SELECT,  sFilter: sFilter);

            //의뢰자 셋팅
            txtUserNameCr.Text = LoginInfo.USERNAME;
            txtUserNameCr.Tag = LoginInfo.USERID;


            #endregion

            #region 팔레트 폐기 이력
            //동
            C1ComboBox[] cboAreaHistoryChild = { cboEquipmentSegmentHistory };
            _combo.SetCombo(cboAreaHistory, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaHistoryChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentHistoryParent = { cboAreaHistory };
            C1ComboBox[] cboEquipmentSegmentHistoryChild = { cboProcessHistory };
            _combo.SetCombo(cboEquipmentSegmentHistory, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentHistoryChild, cbParent: cboEquipmentSegmentHistoryParent);
         

            C1ComboBox[] cbProcessHistoryParent = { cboEquipmentSegmentHistory };
            String[] cbProcesshistory = { "SEARCH", string.Empty, string.Empty, string.Empty };
            _combo.SetCombo(cboProcessHistory, CommonCombo.ComboStatus.ALL, sFilter: cbProcesshistory, sCase: "PROCESS_SORT", cbParent: cbProcessHistoryParent);

            //구분
            string[] sFilterScrapActivity = { "" };
            _combo.SetCombo(cboScrapAct, CommonCombo.ComboStatus.ALL, sFilter: sFilterScrapActivity);

            #endregion


            #region ERP 전송 이력
            //동
            C1ComboBox[] cboAreaErpChild = { cboEquipmentSegmentErp };
            _combo.SetCombo(cboAreaErp, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaErpChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentErpParent = { cboAreaErp };
            C1ComboBox[] cboEquipmentSegmentErpChild = { cboProcessErp };
            _combo.SetCombo(cboEquipmentSegmentErp, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentErpChild, cbParent: cboEquipmentSegmentErpParent);


            C1ComboBox[] cbProcessErpParent = { cboEquipmentSegmentErp };
            String[] cbProcessErp = { "SEARCH", string.Empty, string.Empty, string.Empty };
            _combo.SetCombo(cboProcessErp, CommonCombo.ComboStatus.ALL, sFilter: cbProcessErp, sCase: "PROCESS_SORT", cbParent: cbProcessErpParent);

            #endregion
        }


        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                Array ary = FrameOperation.Parameters;

                DataTable dtInfo = ary.GetValue(0) as DataTable;

                foreach (DataRow dr in dtInfo.Rows)
                {
                    txtPalletIdScrap.Text = Util.NVC(dr["PALLETID"]);
                    GetLotList(false);
                }
            }
            

            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnPalletScrap);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            this.Loaded -= UserControl_Loaded;
        }
      
        #region 팔레트 폐기
        private void btnSearchScrap_Click(object sender, RoutedEventArgs e)
        {
            GetLotList(true);
        }
        //Pallet ID 엔터시
        private void txtPalletIdScrap_KeyDown(object sender, KeyEventArgs e)
        {
            
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLotid = txtPalletIdScrap.Text.Trim();
                    if (dgListInput.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListInput.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListInput.Rows[i].DataItem, "PALLETID").ToString() == sLotid)
                            {
                              
                                dgListInput.SelectedIndex = i;
                                dgListInput.ScrollIntoView(i, dgListInput.Columns["CHK"].Index);
                                txtPalletIdScrap.Focus();
                                txtPalletIdScrap.Text = string.Empty;
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
        //팔레트 폐기
        private void btnPalletScrap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    return;
                }
                //폐기하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4191"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                ScrapPallet();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
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

        private void btnReSet_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListInput);
            Util.gridClear(dgSelectInput);
            cboActivitiReason.SelectedIndex = 0;
            txtUserNameCr.Text = LoginInfo.USERNAME;
            txtUserNameCr.Tag = LoginInfo.USERID;
            txtReson.Text = string.Empty;
        }

        #region [대상 선택하기]
        //checked, unchecked 이벤트 사용하면 화면에서 사라지고 나타날때도
        //이벤트가 발생함 click 이벤트 잡아서 사용할것

        #region 팔레트 폐기



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
             }


        }
        #endregion

        #endregion

        #endregion
        
        #region 팔레트 폐기 이력
        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {

            GetList_History(true);
           
        }

        //팔레트폐기취소
        private void btnScrapCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCel_Validation())
                {
                    return;
                }
                //팔레트폐기 취소하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4192"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Cancel_PalletScrap();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtPalletHistory_KeyDown(object sender, KeyEventArgs e)
        {

            try
            {
               if (e.Key == Key.Enter)
                {
                    string sLotid = txtPalletHistory.Text.Trim();
                    if (dgListHistory.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListHistory.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "PALLETID").ToString() == sLotid)
                            {
                                dgListHistory.SelectedIndex = i;
                                dgListHistory.ScrollIntoView(i, dgListHistory.Columns["CHK"].Index);

                                txtPalletHistory.Focus();
                                txtPalletHistory.Text = string.Empty;
                                return;
                            }
                        }
                    }
                    GetList_History(false);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }


        }

        #endregion


        #endregion

        #region Mehod

        #region [대상목록 가져오기]

        #region 팔레트 폐기
        public void GetLotList(bool bButton, string sPalletID = "")
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
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SCRAP", typeof(string));
              

                DataRow dr = dtRqst.NewRow();
                if (Util.GetCondition(txtPalletIdScrap).Equals("") && sPalletID.Equals("")) //PalletID 가 없는 경우
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentScrap, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                    if (dr["EQSGID"].Equals("")) return;

                    dr["PROCID"] = cboProcessScrap.SelectedValue.ToString() == string.Empty ? null : cboProcessScrap.SelectedValue.ToString();
                    //if (dr["PROCID"].Equals("")) return;
                    ////dr["PROCID"] = Util.GetCondition(cboProcessScrap, bAllNull: true);

                    dr["PJT_NAME"] = txtPrjtNameScrap.Text;
                    dr["PRODID"] = txtProdidScrap.Text;
                    dr["LOTID_RT"] = txtLotRTDScrap.Text;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SCRAP"] = "N";

                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    //dr["LOTID"] = Util.GetCondition(txtPalletIdScrap);
                    dr["LOTID"] = sPalletID.Equals("") ? Util.GetCondition(txtPalletIdScrap) : sPalletID;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SCRAP"] = "N";
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLTE_MERGER_SPLIT_QTY_LIST", "INDATA", "OUTDATA", dtRqst);



                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListInput, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectInput);
                    cboActivitiReason.SelectedIndex = 0;
                    txtUserNameCr.Text = LoginInfo.USERNAME;
                    txtUserNameCr.Tag = LoginInfo.USERID;
                    txtReson.Text = string.Empty;
                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtPalletIdScrap.Focus();
                            txtPalletIdScrap.Text = string.Empty;
                        }
                    });
                    return;
                }
                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                    if (dtRslt.Rows.Count == 1)
                    {
                       
                        DataTable dtSource = DataTableConverter.Convert(dgListInput.ItemsSource);
                        dtSource.Merge(dtRslt);
                        Util.gridClear(dgListInput);
                        Util.GridSetData(dgListInput, dtSource, FrameOperation, true);
                        DataTableConverter.SetValue(dgListInput.Rows[dgListInput.Rows.Count - 1].DataItem, "CHK", 1);
                        Seting_dgSelectInput(dgListInput.Rows.Count - 1);
                        txtPalletIdScrap.Text = string.Empty;
                        txtPalletIdScrap.Focus();
                    }
                    else
                    {
                        Util.gridClear(dgListInput);
                        Util.GridSetData(dgListInput, dtRslt, FrameOperation, true);
                        txtPalletIdScrap.Text = string.Empty;
                        txtPalletIdScrap.Focus();
                    }


                }
                else
                {
                    Util.GridSetData(dgListInput, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectInput);
                    cboActivitiReason.SelectedIndex = 0;
                    txtUserNameCr.Text = LoginInfo.USERNAME;
                    txtUserNameCr.Tag = LoginInfo.USERID;
                    txtReson.Text = string.Empty;
                }
            
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
      
        #endregion

        #region [팔레트 폐기 이력]
        public void GetList_History(bool bButton)
        {
            try
            {
                // [추가]남경 , 오창 조회 DA 분리, 동 선택 안 했을 시 , 메시지 처리 2019.04.03 이제섭
                string bizRuleName = LoginInfo.CFG_SHOP_ID.Equals("G182") ? "DA_PRD_SEL_PALLTE_SCRAP_HISTORY_NJ" : "DA_PRD_SEL_PALLTE_SCRAP_HISTORY";

                string actid = string.Empty;
                if (Util.NVC(cboScrapAct.SelectedValue) == string.Empty)
                {
                    actid = LoginInfo.CFG_SHOP_ID.Equals("G182") ? "SCRAP_LOT,CANCEL_SCRAP_LOT" : null;
                }
                else
                {
                    actid = Util.NVC(cboScrapAct.SelectedValue);
                }

                if (cboAreaHistory.SelectedValue.ToString() == string.Empty || cboAreaHistory.SelectedValue.ToString() == "SELECT")
                {
                    //동을 선택하세요.
                    Util.MessageInfo("SFU1499"); 
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("FROM_DATE", typeof(String));
                dtRqst.Columns.Add("TO_DATE", typeof(String));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("ACTID", typeof(string));

                //ldpDateFromHist.SelectedDateTime.ToShortDateString();
                DataRow dr = dtRqst.NewRow();
                if (Util.GetCondition(txtPalletHistory).Equals("")) //PalletID 가 없는 경우
                {
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                    //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHistory, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                    //if (dr["EQSGID"].Equals("")) return;
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHistory, bAllNull: true);
                    dr["PROCID"] = Util.GetCondition(cboProcessHistory, bAllNull: true);
                    dr["PJT_NAME"] = txtPjtHistory.Text;
                    dr["PRODID"] = txtProdID.Text;
                    dr["LOTID_RT"] = txtLotHistory.Text;
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["ACTID"] = actid;
                }
                else
                {
                    dr["LOTID"] = Util.GetCondition(txtPalletHistory);
                    dr["LANGID"] = LoginInfo.LANGID;
                }
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = null;
                dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    Util.MessageInfo("SFU1905"); //"조회된 Data가 없습니다."
                    Util.gridClear(dgListHistory);
                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtPalletHistory.Focus();
                            txtPalletHistory.Text = string.Empty;
                        }
                    });
                    return;
                }

                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                    if (dtRslt.Rows.Count == 1)
                    {

                        DataTable dtSource = DataTableConverter.Convert(dgListHistory.ItemsSource);
                        dtSource.Merge(dtRslt);
                        Util.gridClear(dgListHistory);
                        Util.GridSetData(dgListHistory, dtSource, FrameOperation, true);
                        txtPalletHistory.Text = string.Empty;
                        txtPalletHistory.Focus();
                        dgListHistory.Columns["RESNQTY"].Width = new C1.WPF.DataGrid.DataGridLength(90);
                    }
                    else
                    {
                        Util.gridClear(dgListHistory);
                        Util.GridSetData(dgListHistory, dtRslt, FrameOperation, true);
                        txtPalletHistory.Text = string.Empty;
                        txtPalletHistory.Focus();
                        dgListHistory.Columns["RESNQTY"].Width = new C1.WPF.DataGrid.DataGridLength(90);
                    }

                }
                else
                {
                    Util.GridSetData(dgListHistory, dtRslt, FrameOperation, true);
                    Util.gridClear(dgListHistory);
                    dgListHistory.Columns["RESNQTY"].Width = new C1.WPF.DataGrid.DataGridLength(90);
                }
         
                //dgListHistory.ItemsSource = DataTableConverter.Convert(dtRslt);
                Util.GridSetData(dgListHistory, dtRslt, FrameOperation, true);
                dgListHistory.Columns["RESNQTY"].Width = new C1.WPF.DataGrid.DataGridLength(90);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


    
       
        #endregion

        #endregion

        #region [팔레트 폐기]
        private void ScrapPallet()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            

            DataRow row = null;
            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["USERID"] = txtUserNameCr.Tag.ToString();
            row["IFMODE"] = "OFF";
            inDataTable.Rows.Add(row);

            //LOT 정보
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("SCRAPQTY", typeof(string));
            inLot.Columns.Add("SCRAPQTY2", typeof(string));
            inLot.Columns.Add("WIPNOTE", typeof(string));
            for (int i = 0; i < dgSelectInput.Rows.Count; i++)
            {

                row = inLot.NewRow();
                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "PALLETID"));
                row["SCRAPQTY"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "WIPQTY"));
                row["SCRAPQTY2"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "WIPQTY"));
                row["WIPNOTE"] = txtReson.Text;
                inLot.Rows.Add(row);

            }
            //INRESN
            DataTable inInresn = inData.Tables.Add("INRESN");
            inInresn.Columns.Add("LOTID",         typeof(string));
            inInresn.Columns.Add("RESNCODE",      typeof(string));
            inInresn.Columns.Add("RESNCODE_CAUSE",typeof(string));
            inInresn.Columns.Add("PROCID_CAUSE",  typeof(string));
            inInresn.Columns.Add("RESNNOTE",      typeof(string));
            for (int i = 0; i < dgSelectInput.Rows.Count; i++)
            {
                row = inInresn.NewRow();
                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "PALLETID"));
                if (cboActivitiReason.SelectedValue.ToString() != "SELECT")
                {
                    row["RESNCODE"] = cboActivitiReason.SelectedValue.ToString();
                }
                row["RESNCODE_CAUSE"] = null;
                row["PROCID_CAUSE"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "PROCID"));
                row["RESNNOTE"] = txtReson.Text;
                inInresn.Rows.Add(row);
            }
            try
            {
                //팔레트  처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SCRAP_PALLET", "INDATA,INLOT,INRESN", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Util.gridClear(dgListInput);
                            Util.gridClear(dgSelectInput);
                            cboActivitiReason.SelectedIndex = 0;
                            txtUserNameCr.Text = string.Empty;
                            txtUserNameCr.Tag = string.Empty;
                            txtReson.Text = string.Empty;
                            GetLotList(true);
                        }
                    });
                    return;
                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_SCRAP_PALLET", ex.Message, ex.ToString());

            }
        }

        public void Seting_dgSelectInput(int seleted_row)
        {
            DataTable dtTo = DataTableConverter.Convert(dgSelectInput.ItemsSource);

            if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTo.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));
                dtTo.Columns.Add("PALLETID", typeof(string));
                dtTo.Columns.Add("FORM_WRK_TYPE_NAME", typeof(string));
                dtTo.Columns.Add("LOTID_RT", typeof(string));
                dtTo.Columns.Add("LOTYNAME", typeof(string));
                dtTo.Columns.Add("PJT", typeof(string));
                dtTo.Columns.Add("PRODID", typeof(string));
                dtTo.Columns.Add("SOC_VALUE", typeof(string));
                dtTo.Columns.Add("WND_GR_CODE", typeof(string));
                dtTo.Columns.Add("WND_EQPTNM", typeof(string));
                dtTo.Columns.Add("WIPQTY", typeof(string));
                dtTo.Columns.Add("UNIT", typeof(string));
                dtTo.Columns.Add("UPDDATE", typeof(string));
                dtTo.Columns.Add("CAPA_GRD_CODE", typeof(string));
                dtTo.Columns.Add("WIP_QLTY_TYPE_NAME", typeof(string));
                dtTo.Columns.Add("PROCID", typeof(string));
                dtTo.Columns.Add("EQSGID", typeof(string));
            }

            if (dtTo.Rows.Count > 0)
            {


                if (dtTo.Select("PROCID = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PROCID") + "'").Length == 0) //동일한 공정이 아닙니다. 
                {
                    DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);
                    Util.MessageValidation("동일한 공정이 아닙니다. ");
                    return;
                }


            }
            DataRow dr = dtTo.NewRow();
            foreach (DataColumn dc in dtTo.Columns)
            {
                dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, dc.ColumnName));
            }
            dtTo.Rows.Add(dr);
            dgSelectInput.ItemsSource = DataTableConverter.Convert(dtTo);
        }

        #endregion

        #region [팔레트 폐기 이력]
        private void Cancel_PalletScrap()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));


            DataRow row = null;
            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["USERID"] = LoginInfo.USERID;
            row["IFMODE"] = "OFF";
            inDataTable.Rows.Add(row);

            //LOT 정보
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("SCRAPQTY", typeof(string));
            inLot.Columns.Add("SCRAPQTY2", typeof(string));
            inLot.Columns.Add("WIPNOTE", typeof(string));
            for (int i = 0; i < dgListHistory.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "CHK")) == "1")
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "PALLETID"));
                    row["SCRAPQTY"] = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "RESNQTY"));
                    row["SCRAPQTY2"] = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "RESNQTY"));
                    row["WIPNOTE"] = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "RESNNOTE"));
                    inLot.Rows.Add(row);
                }
               

            }
            //INRESN
            DataTable inInresn = inData.Tables.Add("INRESN");
            inInresn.Columns.Add("LOTID", typeof(string));
            inInresn.Columns.Add("RESNCODE", typeof(string));
            inInresn.Columns.Add("RESNCODE_CAUSE", typeof(string));
            inInresn.Columns.Add("PROCID_CAUSE", typeof(string));
            inInresn.Columns.Add("RESNNOTE", typeof(string));
            for (int i = 0; i < dgListHistory.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "CHK")) == "1")
                {
                    row = inInresn.NewRow();
                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "PALLETID"));
                    row["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "RESNCODE"));
                    row["RESNCODE_CAUSE"] = null;
                    row["PROCID_CAUSE"] = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "PROCID"));
                    row["RESNNOTE"] = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "RESNNOTE"));
                    inInresn.Rows.Add(row);
                }
            }
            try
            {
                //제품의뢰 처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_SCRAP_PALLET", "INDATA,INLOT,INRESN", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            Util.gridClear(dgListHistory);
                            GetList_History(true);
                        }
                    });
                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_CANCEL_SCRAP_PALLET", ex.Message, ex.ToString());

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
        #endregion


        #endregion

        #region [Validation]

        #region [팔레트 폐기]
        private bool Validation()
        {
          
            if (string.IsNullOrWhiteSpace(txtUserNameCr.Text) || txtUserNameCr.Tag == null)
            {
                Util.MessageValidation("SFU4151"); //의뢰자를 선택하세요
                return false;
            }


            if (cboActivitiReason.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU4193"); //폐기사유코드를 선택하세요
                return false;
            }

            if (dgSelectInput.Rows.Count==0)
            {
                Util.MessageValidation("SFU4194"); //폐기할 데이터가 없습니다.
                return false;
            }
           return true;
        }
        #endregion

        #region [팔레트 폐기 이력]
        private bool CanCel_Validation()
        {

           if (dgListHistory.Rows.Count == 0)
            {
                Util.MessageValidation("SFU1905"); //조회된 데이터가 없습니다.
                return false;
            }

            int CheckCount = 0;

            for (int i = 0; i < dgListHistory.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "CHK")) == "1")
                {
                    CheckCount = CheckCount + 1;
                }
            }

            if (CheckCount  == 0)
            {
                Util.MessageValidation("SFU1651");//선택된 항목이 없습니다.
                return false;
            }
          
            return true;
        }



        #endregion

        #endregion

        private void dgListHistory_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LAST_SCRAP_FLAG")).Equals("Y"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                       
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgListHistory_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgListHistory_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name == "CHK")
            {
                if (DataTableConverter.GetValue(dgListHistory.Rows[e.Row.Index].DataItem, "LAST_SCRAP_FLAG").ToString() == "N")
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
        }

        private void txtPalletIdScrap_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (string.IsNullOrEmpty(sPasteStrings[i]))
                            break;

                        if (dgListInput.GetRowCount() > 0)
                        {
                            for (int idx = 0; idx < dgListInput.GetRowCount(); idx++)
                            {
                                if (DataTableConverter.GetValue(dgListInput.Rows[idx].DataItem, "PALLETID").ToString() == sPasteStrings[i])
                                {
                                    dgListInput.ScrollIntoView(i, dgListInput.Columns["CHK"].Index);
                                    dgListInput.SelectedIndex = i;
                                    txtPalletIdScrap.Focus();
                                    txtPalletIdScrap.Text = string.Empty;
                                    return;
                                }
                            }
                        }

                        GetLotList(false, sPasteStrings[i]);
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
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
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

        private void txtPalletErp_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetList_Erp();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSearchErp_Click(object sender, RoutedEventArgs e)
        {
            GetList_Erp();
        }

        public void GetList_Erp()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(String));
                dtRqst.Columns.Add("TO_DATE", typeof(String));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                
                DataRow dr = dtRqst.NewRow();
                if (Util.GetCondition(txtPalletErp).Equals("")) //PalletID 가 없는 경우
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFromErp);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateToErp);
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentErp, bAllNull: true);
                    dr["PROCID"] = Util.GetCondition(cboProcessErp, bAllNull: true);
                    dr["PRODID"] = Util.ConvertEmptyToNull(txtProdIDErp.Text);
                    dr["LOTID_RT"] = Util.ConvertEmptyToNull(txtLotErp.Text);
                    dr["PJT_NAME"] = Util.ConvertEmptyToNull(txtPjtErp.Text);
                }
                else
                {
                    dr["LOTID"] = Util.GetCondition(txtPalletErp);
                    dr["LANGID"] = LoginInfo.LANGID;
                }
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = null;
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLTE_SCRAP_ERP", "INDATA", "OUTDATA", dtRqst);
                if (dtRslt.Rows.Count <= 0)
                {
                    Util.gridClear(dgListErp);
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                }
                else
                {
                    Util.gridClear(dgListErp);
                    Util.GridSetData(dgListErp, dtRslt, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
