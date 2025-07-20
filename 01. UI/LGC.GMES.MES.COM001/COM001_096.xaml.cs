/*************************************************************************************
 Created Date : 2017.08.07
      Creator : 
   Decription : 활성화 재공 현황 조회
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.07  DEVELOPER : Initial Created.
  2017.11.10   오화백   : 시장유형 추가 
  2022.03.07   장희만   : C20211025-000499 GMES 재공현황 생산특이사항 수정
 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_096 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
       
        private int _tagPrintCount;
        string MCC_MCS = string.Empty;
        string _processCode = string.Empty;

        string _pasteStringLot = string.Empty;

        public COM001_096()
        {
            InitializeComponent();
            InitCombo();
          
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
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnRoute);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            //if (FrameOperation.AUTHORITY.Equals("W"))
            //{
            //    dgLotList.Columns["CHK"].Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    dgLotList.Columns["CHK"].Visibility = Visibility.Collapsed;
            //}
            //여기까지 사용자 권한별로 버튼 숨기기
            this.Loaded -= UserControl_Loaded;
        }


        //화면내 combo 셋팅
        private void InitCombo()
        {

            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChild = {cboProcess};
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //공정
            C1ComboBox[] cboLineParent = { cboArea };
            C1ComboBox[] cboLineChild = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, sCase: "AREA_PROCESS_SORT", cbChild: cboLineChild, cbParent: cboLineParent);

            //라인
            C1ComboBox[] cboProcessParent = { cboArea, cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent, sCase: "PROCESS_EQUIPMENT");


            ////라인
            ////C1ComboBox[] cboLineChild = { cboElecType };
            //C1ComboBox[] cboLineParent = { cboArea };
            //C1ComboBox[] cboLineChild = { cboProcess };
            // _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboLineChild, cbParent: cboLineParent);


            //C1ComboBox[] cboProcessParent = {  cboEquipmentSegment };
            //String[] sFilterProcess = { "SEARCH", string.Empty, string.Empty, string.Empty };
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, sFilter: sFilterProcess, cbParent: cboProcessParent, sCase: "PROCESS_SORT");


            String[] sFilterQmsRequest = { "", "FORM_WRK_TYPE_CODE" };
            _combo.SetCombo(cboQlty, CommonCombo.ComboStatus.ALL, sFilter: sFilterQmsRequest, sCase: "COMMCODES");

            if (cboEquipmentSegment.Items.Count > 0) cboEquipmentSegment.SelectedIndex = 0;
            if (cboProcess.Items.Count > 0) cboProcess.SelectedIndex = 0;

           
        }

        #endregion

        #region Event
     
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _pasteStringLot = string.Empty;
            GetStock();
        }

        


        private void dgSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dgSummary.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //if (e.Cell.Column.Name.Equals("PRJT_NAME"))
                //{
                //    if (e.Cell.Row.Index == 0)
                //    {
                //        DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "PRJT_NAME", ObjectDic.Instance.GetObjectName("총 합계"));
                //    }
                //}
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                   

                    //link 색변경
                    if (e.Cell.Column.Name.Equals("PRODID") || e.Cell.Column.Name.Equals("PROCNAME"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIP_QLTY_TYPE_CODE")).Equals("N"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));

                        }

                        
                    }
                }
              

               


            }));
        }

        private void dgSummary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                 C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
              
                GetStockDetail(dg);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void GetStockDetail(C1DataGrid dg)
        {
            if (dg.CurrentColumn.Name.Equals("PRODID") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
            {
                GetDetailLot(Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PROCID")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PRODID")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "WIP_QLTY_TYPE_CODE")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "FORM_WRK_TYPE_CODE")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PROCNAME")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "MKT_TYPE_CODE")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "EQSGID")),
                             dg.CurrentRow.Index
                             );

            }
            else if (dg.CurrentColumn.Name.Equals("PROCNAME") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
            {
                GetDetailLot(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")), "", "", "", Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PROCNAME")),"","", dg.CurrentRow.Index);

            }
        }

        private void GetDetailLot(string sProcId, string sProdId, string sQLTY, string sWRK, string sProcName, string sMTC, string sEQSGID, int Row)
        {
            try
            {

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("MKT_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("PRODID_ALL", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("LOTRT", typeof(string));
                dtRqst.Columns.Add("LOTID_PR_MCC_CHK", typeof(string));
                dtRqst.Columns.Add("LOTID_PR_MCS_CHK", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                if (string.IsNullOrEmpty(_pasteStringLot) == false)
                {
                    dtRqst.Columns.Add("LOTID_IN", typeof(string));
                }

                DataRow dr = dtRqst.NewRow();
                if(sProcName == string.Empty && Row == 2 ) 
                {
                    // 전체선택
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203"); //동은필수입니다.
                    if (dr["AREAID"].Equals("")) return;
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Trim() == "" ? null : Util.GetCondition(cboEquipmentSegment); //Util.GetCondition(cboEquipmentSegment); //Util.GetCondition(cboEquipmentSegment).Trim() ==  "" ? null : Util.GetCondition(cboEquipmentSegment);
                    dr["FORM_WRK_TYPE_CODE"] = Util.GetCondition(cboQlty);
                }
                else if (sProcName == string.Empty && Row != 2) 
                {
                    //공정 전체 - # 클릭시
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203"); //동은필수입니다.
                    if (dr["AREAID"].Equals("")) return;
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Trim() == "" ? null : Util.GetCondition(cboEquipmentSegment);//sEQSGID;//Util.GetCondition(cboEquipmentSegment); //Util.GetCondition(cboEquipmentSegment).Trim() ==  "" ? null : Util.GetCondition(cboEquipmentSegment);
                    dr["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgSummary.Rows[Row+1].DataItem, "PROCID"));
                    dr["FORM_WRK_TYPE_CODE"] = Util.GetCondition(cboQlty);

                }
                else if (sProcName != string.Empty && sProdId == string.Empty) 
                {
                    //공정 전체 - 공정명 클릭시
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203"); //동은필수입니다.
                    if (dr["AREAID"].Equals("")) return;
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Trim() == "" ? null : Util.GetCondition(cboEquipmentSegment); //sEQSGID;//Util.GetCondition(cboEquipmentSegment); //Util.GetCondition(cboEquipmentSegment).Trim() ==  "" ? null : Util.GetCondition(cboEquipmentSegment);
                    dr["PROCID"] = sProcId;
                    dr["FORM_WRK_TYPE_CODE"] = Util.GetCondition(cboQlty);
                }
                else
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203"); //동은필수입니다.
                    if (dr["AREAID"].Equals("")) return;
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Trim() == "" ? null : Util.GetCondition(cboEquipmentSegment); //sEQSGID;//Util.GetCondition(cboEquipmentSegment); //Util.GetCondition(cboEquipmentSegment).Trim() ==  "" ? null : Util.GetCondition(cboEquipmentSegment);
                    dr["PROCID"] = sProcId;
                    dr["PRODID"] = sProdId; //== "" ? null : sProdId;
                    dr["WIP_QLTY_TYPE_CODE"] = sQLTY; //== "" ? null : sQLTY;
                    dr["FORM_WRK_TYPE_CODE"] = sWRK;  //== "" ? null : sWRK;
                    dr["MKT_TYPE_CODE"] = sMTC;  //== "" ? null : sWRK;
                }

                dr["PRODID_ALL"] = Util.GetCondition(txtProdId);
                dr["PRJT_NAME"] = Util.GetCondition(txtPrjtName);

                dr["LOTRT"] = Util.GetCondition(txtLotRt);
                MCC_MCS = MCC_MCS_CHK();
                if (txtLotRt.Text.Trim().Length > 0)
                {
                    
                    if (MCC_MCS == "MCS") //초소형
                    {
                        dr["LOTID_PR_MCS_CHK"] = "Y";
                    }
                    else  //원형
                    {
                        dr["LOTID_PR_MCC_CHK"] = "Y";
                    }

                }

                dr["LOTID"] = Util.GetCondition(txtPalletId);

                if (string.IsNullOrEmpty(_pasteStringLot) == false)
                {
                    dr["LOTID_IN"] = _pasteStringLot.Trim();
                }

                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new DataTable();
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_STOCK_IN_AREA_DETAIL_TO", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);

                txPalletCnt.Value = dgLotList.Rows.Count;
                txtCellQtySum.Value = Util.NVC_Int(dtRslt.Compute("SUM(WIPQTY)", string.Empty));
                
                _processCode = sProcId;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

  


        #region [대상 선택하기]
        private void dgLotList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e == null || e.Column == null || e.Column.Name == null || e.Column.HeaderPresenter == null) return;

                if (pre == null) return;

                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));

        }

        private void chkWip_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1))//체크되는 경우
            {
                DataRow[] drUnchk = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = 0");

                if (drUnchk.Length == 0)
                {
                    chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                    chkAll.IsChecked = true;
                    chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                }

            }
            else//체크 풀릴때
            {
                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                chkAll.IsChecked = false;
                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
            }
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgLotList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgLotList.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgLotList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgLotList.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();
        }

        #endregion

        #region 태그 발행
        private void btnTagPrint_Click(object sender, RoutedEventArgs e)
        {
            if (dgLotList.ItemsSource == null) {
                Util.MessageValidation("SFU1651");
                return;
            }

            DataRow[] drSelect = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = 1");

            if (drSelect.Length == 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }
            
             _tagPrintCount = drSelect.Length;
           
            foreach (DataRow drPrint in drSelect)
            {

                if (drPrint["CLSS3_CODE"].ToString() == "MCC" || drPrint["CLSS3_CODE"].ToString() == "MCM")
                {
                    TagPrint(drPrint);
                }
                else if (drPrint["CLSS3_CODE"].ToString() == "MCS")
                {
                    TagPrint(drPrint);
                }
                else if (drPrint["CLSS3_CODE"].ToString() == "MCR")
                {
                    TagPrint(drPrint);
                }
                else
                {
                    
                    POLYMER_TagPrint(drPrint);
                }
                  
            }
        }

        private void TagPrint(DataRow dr)
        {
            CMM_FORM_TAG_PRINT popupTagPrint = new CMM_FORM_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;
            if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5")) //남경
            {
                if (dr["CLSS3_CODE"].ToString() == "MCS") //초소형
                {
                    if (dr["WIP_QLTY_TYPE_CODE"].ToString() == "N")
                    {
                        popupTagPrint.DefectPalletYN = "Y";
                    }
                    else
                    {
                        popupTagPrint.QMSRequestPalletYN = "Y";
                    }
                }
                else
                {
                    if(dr["WIP_QLTY_TYPE_CODE"].ToString() == "N")
                    {
                        popupTagPrint.DefectPalletYN = "Y";
                    }
                    else
                    {
                        if(dr["PROCID"].Equals(Process.CircularCharacteristicGrader))
                        {
                            popupTagPrint.QMSRequestPalletYN = "Y";
                        }
                        else
                        {
                            popupTagPrint.returnPalletYN = "Y";
                        }
                      
                    }
                  
                }
                
            }
            else 
            {
                if (dr["WIP_QLTY_TYPE_CODE"].ToString() == "N")
                {
                    popupTagPrint.DefectPalletYN = "Y";
                }
                else
                {
                    popupTagPrint.QMSRequestPalletYN = "Y";
                }

            }
            popupTagPrint.PrintCount = _tagPrintCount.ToString();

            _tagPrintCount--;

            object[] parameters = new object[8];
            parameters[0] = dr["PROCID"];
            parameters[1] = null;              // 설비ID
            parameters[2] = dr["LOTID"];
            parameters[3] = dr["WIPSEQ"].ToString();
            parameters[4] = dr["WIPQTY"].ToString();
            parameters[5] = "N";                                         // 디스패치 처리
            parameters[6] = "Y";                                         // 출력 여부
            parameters[7] = (bool)chkTagPrint.IsChecked ? "N" : "Y";     // Direct 출력 여부

            C1WindowExtension.SetParameters(popupTagPrint, parameters);

            popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);
            grdMain.Children.Add(popupTagPrint);
            popupTagPrint.BringToFront();
        }

        private void popupTagPrint_Closed(object sender, EventArgs e)
        {
            CMM_FORM_TAG_PRINT popup = sender as CMM_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(popup);
        }



        private void POLYMER_TagPrint(DataRow dr)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupTagPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;
            popupTagPrint.CART_MERGE = "Y";
            popupTagPrint.PrintCount = _tagPrintCount.ToString();

            _tagPrintCount--;

            object[] parameters = new object[5];
            parameters[0] = "";       // _processCode;
            parameters[1] = null;     // Util.NVC(ComboEquipment.SelectedValue);
            parameters[2] = dr["CTNR_ID"];
            parameters[3] = "N";      // Direct 출력 여부
            parameters[4] = "N";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupTagPrint, parameters);

            popupTagPrint.Closed += new EventHandler(POLYMER_popupTagPrint_Closed);
            grdMain.Children.Add(popupTagPrint);
            popupTagPrint.BringToFront();
        }
        private void POLYMER_popupTagPrint_Closed(object sender, EventArgs e)
        {
            CMM_FORM_TAG_PRINT popup = sender as CMM_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(popup);
        }

        #endregion


        #endregion

        #region Method
        private void GetStock()
        {
            try
            {
                //string sBizName = chkProdVerCode.IsChecked == true ? "DA_PRD_SEL_STOCK_IN_AREA_VER" : "DA_PRD_SEL_STOCK_IN_AREA";
                //string sBizName = chkProdVerCode.IsChecked == true ? "DA_PRD_SEL_STOCK_IN_AREA_VER_TO" : "DA_PRD_SEL_STOCK_IN_AREA_TO";
                string sBizName = "DA_PRD_SEL_FORM_STOCK_IN_AREA_TO";
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("LOTRT", typeof(string));
                dtRqst.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("LOTID_PR_MCC_CHK", typeof(string));
                dtRqst.Columns.Add("LOTID_PR_MCS_CHK", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                if (string.IsNullOrEmpty(_pasteStringLot) == false)
                {
                    dtRqst.Columns.Add("LOTID_IN", typeof(string));
                }

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();

                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203");//동은필수입니다.
                if (dr["AREAID"].ToString().Equals("")) return;
                //dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "SFU1223");//라인은 필수 입니다
                //if (dr["EQSGID"].Equals("")) return;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                dr["PROCID"] = Util.GetCondition(cboProcess);
                dr["PRODID"] = Util.GetCondition(txtProdId);
                dr["PRJT_NAME"] = Util.GetCondition(txtPrjtName);
                dr["LOTRT"] = Util.GetCondition(txtLotRt);
                dr["FORM_WRK_TYPE_CODE"] = Util.GetCondition(cboQlty);
                if(txtLotRt.Text.Trim().Length > 0 )
                {
                    MCC_MCS = MCC_MCS_CHK();
                    if (MCC_MCS == "MCS") //초소형
                    {
                        dr["LOTID_PR_MCS_CHK"] = "Y";
                    }
                    else  //원형
                    {
                        dr["LOTID_PR_MCC_CHK"] = "Y";
                    }
                   
                }
                dr["LOTID"] = Util.GetCondition(txtPalletId);

                if (string.IsNullOrEmpty(_pasteStringLot) == false)
                {
                    dr["LOTID_IN"] = _pasteStringLot.Trim();
                }

                dtRqst.Rows.Add(dr);
                dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);
                
                Util.GridSetData(dgSummary, dtRslt, FrameOperation, true);
                Util.gridClear(dgLotList);

                txPalletCnt.Value = 0;
                txtCellQtySum.Value = 0;

                if (dtRslt.Rows.Count > 0)
                {
                    string[] sColumnName = new string[] { "PROCNAME", "PRJT_NAME" };
                    _Util.SetDataGridMergeExtensionCol(dgSummary, sColumnName, DataGridMergeMode.VERTICAL);

                    dgSummary.GroupBy(dgSummary.Columns["PROCNAME"], DataGridSortDirection.None);
                    dgSummary.GroupRowPosition = DataGridGroupRowPosition.AboveData;
                    
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRJT_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("합계") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRODID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["FORM_WRK_TYPE_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WIP_QLTY_TYPE_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MKT_TYPE_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["UNIT_CODE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                  
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                   
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                  
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_LOT_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_LOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                          
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_CNT"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                }
              
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private string MCC_MCS_CHK()
        {
            string ReturnValue = string.Empty;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQSG_CS_CR", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    ReturnValue = dtRslt.Rows[0]["S04"].ToString();
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }


            return ReturnValue;

        }


        #endregion

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            new LGC.GMES.MES.Common.ExcelExporter().Export(dgLotList);
        }

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgSummary_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void dgLotList_AutoGeneratedColumns(object sender, EventArgs e)
        {
            dgLotList.GroupBy(dgLotList.Columns["HOLD"]);
            dgLotList.FilterBy(dgLotList.Columns["HOLD"], new DataGridFilterState()
            {
                FilterInfo = new List<DataGridFilterInfo>(
                    new DataGridFilterInfo[1]
                    {
                        new DataGridFilterInfo()
                        {
                            FilterOperation = DataGridFilterOperation.Contains, FilterType = DataGridFilterType.Text, Value = "Y"
                        }
                    })
            });
        }

        private void btnWipRemarks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgLotList.ItemsSource == null)
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }

                DataRow[] drSelect = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = 1");

                if (drSelect.Length == 0)
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }

                COM001_096_WIP_REMARKS popupWipRemarks = new COM001_096_WIP_REMARKS();
                popupWipRemarks.FrameOperation = this.FrameOperation;

                object[] parameters = new object[1];
                parameters[0] = drSelect;

                C1WindowExtension.SetParameters(popupWipRemarks, parameters);

                popupWipRemarks.Closed += new EventHandler(popupWipRemarks_Closed);

                grdMain.Children.Add(popupWipRemarks);
                popupWipRemarks.BringToFront();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }

        private void popupWipRemarks_Closed(object sender, EventArgs e)
        {
            COM001_096_WIP_REMARKS popupWipRemarks = sender as COM001_096_WIP_REMARKS;
            
            if (popupWipRemarks != null && popupWipRemarks.DialogResult == MessageBoxResult.OK)
            {
                GetStockDetail(dgSummary);
            }
            this.grdMain.Children.Remove(popupWipRemarks);
        }

        private void popupPrdRemarks_Closed(object sender, EventArgs e)
        {
            COM001_096_PRD_REMARKS popupPrdRemarks = sender as COM001_096_PRD_REMARKS;

            if (popupPrdRemarks != null && popupPrdRemarks.DialogResult == MessageBoxResult.OK)
            {
                GetStockDetail(dgSummary);
            }
            this.grdMain.Children.Remove(popupPrdRemarks);
        }

        private void dgLotList_FilterChanged(object sender, DataGridFilterChangedEventArgs e)
        {
            txPalletCnt.Value = dgLotList.Rows.Count;
            txtCellQtySum.Value = Util.NVC_Int(DataTableConverter.Convert(dgLotList.ItemsSource).Compute("SUM(WIPQTY)", string.Empty));
        }

        private void txtPalletId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                _pasteStringLot = string.Empty;

                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    if(sPasteStrings.Length > 0)
                    {
                        for (int inx = 0; inx < sPasteStrings.Length; inx++)
                        {
                            //중간에 공백 있으면 안됨. 마지막 에는 공백 있어도 됨
                            if (inx != sPasteStrings.Length - 1 && 
                                (string.IsNullOrWhiteSpace(sPasteStrings[inx]) || string.IsNullOrEmpty(sPasteStrings[inx])))
                            {
                                Util.MessageValidation("SFU1362");  //LOT ID 형식이 올바르지 않습니다. 확인 후 다시 등록해 주세요.
                                return;
                            }

                            _pasteStringLot += sPasteStrings[inx].Trim() + ",";
                        }

                        GetStock();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }

        }

        private void btnPrdRemarks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgLotList.ItemsSource == null)
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }

                DataRow[] drSelect = DataTableConverter.Convert(dgLotList.ItemsSource).Select("CHK = 1");

                if (drSelect.Length == 0)
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }

                COM001_096_PRD_REMARKS popupPrdRemarks = new COM001_096_PRD_REMARKS();
                popupPrdRemarks.FrameOperation = this.FrameOperation;

                object[] parameters = new object[1];
                parameters[0] = drSelect;

                C1WindowExtension.SetParameters(popupPrdRemarks, parameters);

                popupPrdRemarks.Closed += new EventHandler(popupPrdRemarks_Closed);

                grdMain.Children.Add(popupPrdRemarks);
                popupPrdRemarks.BringToFront();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }
    }
}
