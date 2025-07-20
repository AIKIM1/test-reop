/*************************************************************************************
 Created Date : 2017.03.07
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.07  DEVELOPER : Initial Created.
  2023.03.30  이홍주    : 소형활성화MES 복사

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
 
namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_323 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();
        public FCS002_323()
        {
            InitializeComponent();

            InitCombo();
        }

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

            // 2018-08-29 오창일 경우 동 라인 공정 이고 그외는 동 공정 라인으로 변경된다
            
            if ("A010".Equals(LoginInfo.CFG_SHOP_ID) == true)
            {
                Line.Visibility = Visibility.Visible;
                Process.Visibility = Visibility.Visible;
                Line_Etc.Visibility = Visibility.Collapsed;
                Process_Etc.Visibility = Visibility.Collapsed;

            }
            else
            {
                Line.Visibility = Visibility.Collapsed;
                Process.Visibility = Visibility.Collapsed;
                Line_Etc.Visibility = Visibility.Visible;
                Process_Etc.Visibility = Visibility.Visible;
            }
            
            //동.
            //C20180122_88761 : [CSR ID:3588761] 활성화 Pallet별 생산실적 조회 (1동2동 일괄조회)
            if ("A010".Equals(LoginInfo.CFG_SHOP_ID) == true)
            {
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild);
            }else
            {
                C1ComboBox[] cboAreaChild = { cboProcess_Etc };
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);
            }

          
            //C20180122_88761 : [CSR ID:3588761] 활성화 Pallet별 생산실적 조회 (1동2동 일괄조회)
            if ("A010".Equals(LoginInfo.CFG_SHOP_ID) == true)
            {
                //라인
                C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
                C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            }
            else
            {
                //공정
                C1ComboBox[] cboLineParent = { cboArea };
                C1ComboBox[] cboLineChild = { cboEquipmentSegment_Etc, cboEquipment };
                _combo.SetCombo(cboProcess_Etc, CommonCombo.ComboStatus.SELECT, sCase: "AREA_PROCESS_SORT", cbChild: cboLineChild, cbParent: cboLineParent);

                //C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
                //C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
                //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            }

            if ("A010".Equals(LoginInfo.CFG_SHOP_ID) == true)
            {
                //공정
                C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
                C1ComboBox[] cbProcessChild = { cboEquipment };
                String[] cbProcess = { "SEARCH", string.Empty, string.Empty, string.Empty };
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, sFilter: cbProcess, sCase: "PROCESS_SORT", cbParent: cbProcessParent, cbChild: cbProcessChild);
            }
            else
            {
                //라인
                C1ComboBox[] cboProcessParent = { cboArea, cboProcess_Etc };
                C1ComboBox[] cbProcessChild = { cboEquipment };
                _combo.SetCombo(cboEquipmentSegment_Etc, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent, sCase: "PROCESS_EQUIPMENT");
            }


            if ("A010".Equals(LoginInfo.CFG_SHOP_ID) == true)
            {
                //설비
                C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);
            }
            else
            {
                //설비
                C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment_Etc, cboProcess_Etc };
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

                /////
                ////2023.06.13 수정시작
                //private void SetEquipmentCombo(C1ComboBox cbo)
                //{
                //    //string _processCodeTemp = "FF5101, FFV101";
                //    /// B1000"; //2023.06.02 TEST
                //    /// F5300으로 감

                //    string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
                //    if (LoginInfo.CFG_SHOP_ID == "G182")
                //        bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO_MB";

                //    string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "COATER_EQPT_TYPE_CODE" };
                //    string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_EQSG_ID, _processCode, null };
                //    //string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_EQSG_ID, _processCodeTemp, null };
                //    string selectedValueText = cbo.SelectedValuePath;
                //    string displayMemberText = cbo.DisplayMemberPath;

                //    CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);

                //    //////////////////// 설비가 N건인 경우 Select 추가
                //    if (ComboEquipment.Items.Count > 1 || ComboEquipment.Items.Count == 0)
                //    {
                //        DataTable dtEqpt = DataTableConverter.Convert(cbo.ItemsSource);
                //        DataRow drEqpt = dtEqpt.NewRow();
                //        drEqpt[selectedValueText] = "SELECT";
                //        drEqpt[displayMemberText] = "- SELECT -";
                //        dtEqpt.Rows.InsertAt(drEqpt, 0);

                //        cbo.ItemsSource = null;
                //        cbo.ItemsSource = dtEqpt.Copy().AsDataView();

                //        int Index = 0;

                //        if (!string.IsNullOrEmpty(LoginInfo.CFG_EQPT_ID))
                //        {
                //            DataRow[] drIndex = dtEqpt.Select("CBO_CODE ='" + LoginInfo.CFG_EQPT_ID + "'");

                //            if (drIndex.Length > 0)
                //            {
                //                Index = dtEqpt.Rows.IndexOf(drIndex[0]);
                //                cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;
                //            }
                //        }

                //        cbo.SelectedIndex = Index;
                //    }

                //}


        /////


    }

          
           
            //if(cboEquipment.Items.Count ==2)
            //{
            //    cboEquipment.SelectedIndex = 1;
            //}

            //LOTTYPE
            _combo.SetCombo(cboLotType, CommonCombo.ComboStatus.ALL);
           
            //재공구분
            String[] sFilterQLTY = { "", "WIP_QLTY_TYPE_CODE" };
            _combo.SetCombo(cboQlty, CommonCombo.ComboStatus.ALL, sFilter: sFilterQLTY, sCase: "COMMCODES");

            //용량등급
            String[] sFilterCapa = { "", "CAPA_GRD_CODE" };
            _combo.SetCombo(cboCapaGrd, CommonCombo.ComboStatus.ALL, sFilter: sFilterCapa, sCase: "COMMCODES");

            //전압등급
            String[] sFilterVltg = { "", "VLTG_GRD_CODE" };
            _combo.SetCombo(cboVltgGrd, CommonCombo.ComboStatus.ALL, sFilter: sFilterVltg, sCase: "COMMCODES");

            if ("A010".Equals(LoginInfo.CFG_SHOP_ID) == true)
            {
                //작업조
                String[] sFilterShift = { cboArea.SelectedValue.ToString(), cboEquipmentSegment.SelectedValue.ToString(), cboProcess.SelectedValue.ToString() };
                _combo.SetCombo(cboShift, CommonCombo.ComboStatus.ALL, sFilter: sFilterShift, sCase: "SHIFT");
            }
            else
            {
                //작업조
                String[] sFilterShift = { cboArea.SelectedValue.ToString(), cboEquipmentSegment_Etc.SelectedValue.ToString(), cboProcess_Etc.SelectedValue.ToString() };
                _combo.SetCombo(cboShift, CommonCombo.ComboStatus.ALL, sFilter: sFilterShift, sCase: "SHIFT");
            }
              

            //저항등급
            String[] sFilterRss = { "", "RSST_GRD_CODE" };
            _combo.SetCombo(cboRssGrd, CommonCombo.ComboStatus.ALL, sFilter: sFilterRss, sCase: "COMMCODES");

            //내수/수출 구분
            String[] sFilterMKT = { "", "MKT_TYPE_CODE" };
            _combo.SetCombo(cboMkt, CommonCombo.ComboStatus.ALL, sFilter: sFilterMKT, sCase: "COMMCODES");

            setShipToPopControl();
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                object[] tmps = this.FrameOperation.Parameters;

                dtpDateFrom.SelectedDateTime = Convert.ToDateTime(tmps[0] as string);
                dtpDateTo.SelectedDateTime = Convert.ToDateTime(tmps[0] as string);
                cboArea.SelectedValue = tmps[1] as string;

                if ("A010".Equals(LoginInfo.CFG_SHOP_ID) == true)
                {
                    cboEquipmentSegment.SelectedValue = tmps[2] as string;
                    cboProcess.SelectedValue = tmps[3] as string;
                }
                else
                {
                    cboEquipmentSegment_Etc.SelectedValue = tmps[2] as string;
                    cboProcess_Etc.SelectedValue = tmps[3] as string;
                }
                cboEquipment.SelectedValue = tmps[4] as string;
                txtPJT.Text = tmps[5] as string;
                txtProd.Text = tmps[6] as string;
                txtLotRt.Text = tmps[7] as string;
                cboShift.SelectedValue = tmps[8] as string;
                cboMkt.SelectedValue = tmps[9] as string;
                ClearValue();
                GetResult();
            }
            else
            {
                dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
                dtpDateTo.SelectedDateTime = DateTime.Now;
            }

            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            this.Loaded -= UserControl_Loaded;
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            //{
            //    if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 7)
            //    {
            //        Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
            //        dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime.AddDays(-6);
            //        return;
            //    }

            //    if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
            //    {
            //        dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
            //        return;
            //    }
            //}
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearValue();
            GetResult();
        }

        #endregion

        #region Mehod
        public void GetResult()
        {
            try
            {
                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));
                dtRqst.Columns.Add("WIP_QLTY", typeof(string));
                dtRqst.Columns.Add("CAPA_GRD", typeof(string));
                dtRqst.Columns.Add("VLTG_GRD", typeof(string));
                dtRqst.Columns.Add("SOC", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("SHIFT", typeof(string));
                dtRqst.Columns.Add("MKT_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("RSST_GRD_CODE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("SHIPTO_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                if(txtLotid.Text == string.Empty)
                {
                    //C20180122_88761 : [CSR ID:3588761] 활성화 Pallet별 생산실적 조회 (1동2동 일괄조회)
                    if ("A010".Equals(LoginInfo.CFG_SHOP_ID) == true)
                    {
                        dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                        dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);
                    }
                    else
                    {
                        // 남경 파우치 요청으로 라인 조건 주석 해제 2019.04.11 이제섭
                        dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment_Etc, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                        if (dr["EQSGID"].Equals(""))
                        {
                            Util.MessageValidation("SFU1223");
                            return;
                        }
                        dr["PROCID"] = Util.GetCondition(cboProcess_Etc, "SFU1459"); // 공정을 선택하세요
                        if (dr["PROCID"].Equals(""))
                        {
                            Util.MessageValidation("SFU1459");
                            return;
                        }
                    }

                    //동이 선택되어 있으면 날짜 제한 없고 동이 선택되어 있으면 31일 기간만 조회 가능하도록
                    dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                    if (String.IsNullOrEmpty(dr["AREAID"].ToString()) && (dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                    {
                        Util.MessageValidation("SFU2042", "31");
                        return;
                    }

                    dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                    dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                    dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                    dr["PJT_NAME"] = txtPJT.Text.ToString() == string.Empty ? null : txtPJT.Text.ToString();
                    dr["PRODID"] = txtProd.Text.ToString() == string.Empty ? null : txtProd.Text.ToString();
                    dr["LOTID_RT"] = txtLotRt.Text.ToString() == string.Empty ? null : txtLotRt.Text.ToString();
                    dr["LOTTYPE"] = Util.GetCondition(cboLotType, bAllNull: true);
                    dr["WIP_QLTY"] = Util.GetCondition(cboQlty, bAllNull: true);
                    dr["CAPA_GRD"] = Util.GetCondition(cboCapaGrd, bAllNull: true);
                    dr["VLTG_GRD"] = Util.GetCondition(cboVltgGrd, bAllNull: true);
                    dr["SOC"] = txtSoc.Text.ToString() == string.Empty ? null : txtSoc.Text.ToString();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SHIFT"] = Util.GetCondition(cboShift, bAllNull: true);
                    dr["MKT_TYPE_CODE"] = Util.GetCondition(cboMkt, bAllNull: true);
                    dr["RSST_GRD_CODE"] = Util.GetCondition(cboRssGrd, bAllNull: true);
                    if(popShipto.SelectedValue == null || popShipto.SelectedValue.ToString().Trim() == "")
                    {
                        dr["SHIPTO_ID"] = null;
                    }
                    else
                    {
                        dr["SHIPTO_ID"] = popShipto.SelectedValue.ToString().Trim();
                    }
                    
                }
                else
                {
                    dr["LOTID"] = txtLotid.Text;
                    dr["LANGID"] = LoginInfo.LANGID;
                }
                dtRqst.Rows.Add(dr); 

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_RESULT", "INDATA", "OUTDATA", dtRqst);
                
                Util.gridClear(dgResult);
                //Util.GridSetData(dgResult, dtRslt, FrameOperation);
                if(dtRslt.Rows.Count >0)
                {
                    for(int i=0; i<dtRslt.Rows.Count; i++)
                    {
                        if(dtRslt.Rows[i]["PROC_LOCATE_FLAG"].ToString() == "Y")
                        {
                            dtRslt.Rows[i]["PROC_LOCATE_FLAG"] = ObjectDic.Instance.GetObjectName("재공이동");
                        }
                        else if(dtRslt.Rows[i]["PROC_LOCATE_FLAG"].ToString() == "N")
                        {
                            dtRslt.Rows[i]["PROC_LOCATE_FLAG"] = ObjectDic.Instance.GetObjectName("불량팔레트투입");
                        }


                    }
                }


                Util.GridSetData(dgResult, dtRslt, FrameOperation, true);
                dgResult.Columns["CELLQTY"].Width = new C1.WPF.DataGrid.DataGridLength(120);
                dgResult.Columns["INBOX_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                //string[] sColumnName = new string[] { "PALLETID", "PR_LOTID", "FORM_WRK_TYPE_NAME", "LOTID_RT", "LOTYNAME", "PRJT_NAME", "PRODID", "WIP_QLTY_TYPE_NAME", "CAPA_GRD_CODE", "VLTG_GRD_CODE", "SOC_VALUE", "WND_GR_CODE", "WND_EQPTNM", "INBOX_TYPE_NAME", "WND_EQPTNM", "INBOX_QTY" };
                // _Util.SetDataGridMergeExtensionCol(dgResult, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);



            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #region [초기화]
        private void ClearValue()
        {
            Util.gridClear(dgResult);
        }
        #endregion

        #endregion

        private void txtLotid_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetResult();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgResult_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgResult.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //if(e.Cell.Column.Name == "")

                    //음수수량 색변경
                    if ( DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSEQ").GetDouble() < 0)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    }
                   else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);

                       
                    }
                }
            }));
        }

        private void dgResult_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        private void setShipToPopControl()
        {
            const string bizRuleName = "DA_PRD_SEL_SHIPTO_CBO_MB";
            string[] arrColumn = { "SHOPID", "PRODID", "LANGID" };
            string[] arrCondition = { LoginInfo.CFG_SHOP_ID, null, LoginInfo.LANGID };
            CommonCombo.SetFindPopupCombo(bizRuleName, popShipto, arrColumn, arrCondition, (string)popShipto.SelectedValuePath, (string)popShipto.DisplayMemberPath);
        }


    }
}
