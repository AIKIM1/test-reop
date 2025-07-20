/*************************************************************************************
 Created Date : 2017.02.22
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.22  DEVELOPER : Initial Created.
  2018.05.28  JMK 폐기 및 양품화 관련 수정




 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;


namespace LGC.GMES.MES.COM001
{
    public partial class COM001_221 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        string _selectDefectCart = string.Empty;
        string _selectScrapCart = string.Empty;
        bool _select;

        public COM001_221()
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

        /// <summary>
        /// Form Load
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnDefectCtnlInput);
            listAuth.Add(btnDefectInput);
            listAuth.Add(btnDefectDelete);
            listAuth.Add(btnDefectLoss);
            listAuth.Add(btnDefectGood);
            listAuth.Add(btnScrapCtnlInput);
            listAuth.Add(btnScrapCell);
            listAuth.Add(btnScrapInput);
            listAuth.Add(btnScrapCartCancel);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// User Control Clear
        /// </summary>
        private void InitializeUserControls(bool IsAllClear)
        {
            Util.gridClear(dgDefectCtnr);
            Util.gridClear(dgDefectLot);
            Util.gridClear(dgScrapCtnr);
            Util.gridClear(dgScrapLot);

            _selectDefectCart = string.Empty;
            _selectScrapCart = string.Empty;
            _select = true;

            if (IsAllClear)
            {
                txtShift.Text = string.Empty;
                txtWorker.Text = string.Empty;
                txtWorker.Tag = string.Empty;
            }
        }

        /// <summary>
        /// Combo Setting
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            // 동
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboAreaChild = { cboEqsgid };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT);

            //공정
             C1ComboBox[] cboProcessChild = { cboEqsgid };
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS",   cbChild: cboProcessChild);

            String[] sFilter5 = { "", "FORM_PROCID" };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, sFilter: sFilter5, sCase: "COMMCODES", cbChild: cboProcessChild);

            //라인
            C1ComboBox[] cboLineParentParentRegister = {cboArea,cboProcess };
            _combo.SetCombo(cboEqsgid, CommonCombo.ComboStatus.SELECT, sCase: "PROCESS_EQUIPMENT", cbParent: cboLineParentParentRegister);
            //cboEqsgid.SelectedValue = LoginInfo.CFG_EQSG_ID;
        }
        #endregion

        #region Event

        /// <summary>
        /// 공정 선택시 초기화
        /// </summary>
        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            InitializeUserControls(true);
        }

        /// <summary>
        ///  불량/폐기 대차 조회
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            InitializeUserControls(false);

            SearchDefectCtnr(dgDefectCtnr);          // 불량대차 조회
            if (_select)
            {
                SearchDefectCtnr(dgScrapCtnr);           // 폐기대차 조회
            }
        }

        /// <summary>
        /// 상세내용 조회
        /// </summary>
        private void dgDefectCtnrChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
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

                        dgDefectCtnr.SelectedIndex = idx;

                        _selectDefectCart = Util.NVC(DataTableConverter.GetValue(dgDefectCtnr.Rows[idx].DataItem, "CTNR_ID"));
                        SearchDefectDetail(dgDefectLot, _selectDefectCart);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 불량대차등록 팝업
        /// </summary>
        private void btnDefectCtnlInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDefectCtnlInput()) return;

            popUpDefectCtnlInput();
        }

        /// <summary>
        /// 불량추가 팝업
        /// </summary>
        private void btnDefectInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDefectInput()) return;

            popUpDefectInput();
        }

        /// <summary>
        /// 불량삭제 
        /// </summary>
        private void btnDefectDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDefectDelete()) return;

            //// 삭제처리 하시겠습니까?
            //Util.MessageConfirm("SFU1259", (result) =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
                    DefectDelete();
            //    }
            //});
        }

        /// <summary>
        /// 불량LOSS처리 팝업 
        /// </summary>
        private void btnDefectLoss_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDefectLoss()) return;

            popUpDefectLoss();
        }

        /// <summary>
        /// 양품화등록 팝업 
        /// </summary>
        private void btnDefectGood_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDefectGood()) return;

            popUpDefectGood();
        }

        /// <summary>
        /// 폐기대차등록 팝업 
        /// </summary>
        private void btnScrapCtnlInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationScrapCtnlInput()) return;

            popUpScrapCtnr();
        }

        /// <summary>
        /// 상세내용 조회
        /// </summary>
        private void dgScrapCtnrChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
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

                        dgScrapCtnr.SelectedIndex = idx;

                        _selectScrapCart = Util.NVC(DataTableConverter.GetValue(dgScrapCtnr.Rows[idx].DataItem, "CTNR_ID"));
                        SearchDefectDetail(dgScrapLot, _selectScrapCart);
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnScrapCartCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationScrapCartCancel()) return;

            ScrapCartCancel();
        }

        private void btnScrapCellInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationScrapCellInput()) return;

            popUpScrapCellInput();
        }

        private void btnScrapInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationScrapInput()) return;

            //SFU010120593 활성화 대차 폐기처ㅣ
            DataTable dtInfo = DataTableConverter.Convert(dgScrapCtnr.ItemsSource);

            List<DataRow> drList = dtInfo.Select("CHK = '1'").ToList();

            if (drList.Count <= 0)
            {
                // SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            loadingIndicator.Visibility = Visibility.Visible;
            object[] sParam = { drList.CopyToDataTable() };

            this.FrameOperation.OpenMenu("SFU010120593", true, sParam);
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 불량 대차시트 발행
        /// </summary>
        private void btnDefectSheet_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCartRePrint(dgDefectCtnr)) return;

            CartSheetPrint(dgDefectCtnr);
        }

        /// <summary>
        /// 폐기 대차시트 발행
        /// </summary>
        private void btnScrapSheet_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCartRePrint(dgScrapCtnr)) return;

            CartSheetPrint(dgScrapCtnr);
        }

        /// <summary>
        /// 작업조 팝업
        /// </summary>
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            popUpShift();
        }

        #region 비용처리 화면으로 이동 : btnCost_Click()
        ////활성화 비용처리 화면 이동
        //private void btnCost_Click(object sender, RoutedEventArgs e)
        //{

        //    if (!ValidationScrapInput())
        //    {
        //        return;
        //    }
        //    //SFU010120431 활성화 대차 비용처리 대상 등록/취소
        //    DataTable dtInfo = DataTableConverter.Convert(dgDefectCtnr.ItemsSource);

        //    List<DataRow> drList = dtInfo.Select("CHK = '1'").ToList();

        //    if (drList.Count <= 0)
        //    {
        //        // SFU1645 선택된 작업대상이 없습니다.
        //        Util.MessageValidation("SFU1645");
        //        return;
        //    }

        //    loadingIndicator.Visibility = Visibility.Visible;
        //    object[] sParam = { drList.CopyToDataTable() };

        //    this.FrameOperation.OpenMenu("SFU010120431", true, sParam);
        //    loadingIndicator.Visibility = Visibility.Collapsed;
        //}
        #endregion

        #endregion

        #region Method

        #region [Biz]
        /// <summary>
        /// 불량대차 조회
        /// </summary>
        private void SearchDefectCtnr(C1DataGrid dg)
        {
            try
            {
                string SelectCartID = string.Empty;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                dtRqst.Columns.Add("DEFECT", typeof(string));
                dtRqst.Columns.Add("SCRAP", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["AREAID"] = Util.GetCondition(cboArea, "SFU4238");     // 동 정보는필수 입니다.
                if (dr["AREAID"].Equals(""))
                {
                    _select = false;
                    return;
                }
                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459");  // 공정은 필수 입니다.
                if (dr["PROCID"].Equals(""))
                {
                    _select = false;
                    return;
                }

                dr["EQSGID"] = Util.GetCondition(cboEqsgid, "SFU4050");   // 라인을 선택해주세요
                if (dr["EQSGID"].Equals(""))
                {
                    _select = false;
                    return;
                }

                dr["PJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtName.Text.Trim());
                dr["PRODID"] = Util.ConvertEmptyToNull(txtProdId.Text.Trim());
                dr["CTNR_ID"] = Util.ConvertEmptyToNull(txtCtnrId.Text.Trim());

                if (dg.Name.Equals("dgDefectCtnr"))
                {
                    dr["DEFECT"] = "Y";
                    SelectCartID = _selectDefectCart;
                }
                else
                {
                    dr["SCRAP"] = "Y";
                    SelectCartID = _selectScrapCart;
                }

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_FORMATION_DEFECT_CART", "INDATA", "OUTDATA", dtRqst, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dg, bizResult, FrameOperation, true);

                        int idx = _Util.GetDataGridRowIndex(dg, "CTNR_ID", SelectCartID);
                        if (idx >= 0)
                        {
                            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", true);

                            // row 색 바꾸기
                            dg.SelectedIndex = idx;
                            dg.CurrentCell = dg.GetCell(idx, dg.Columns.Count - 1);

                            if (dg.Name.Equals("dgDefectCtnr"))
                                SearchDefectDetail(dgDefectLot, SelectCartID);
                            else
                                SearchDefectDetail(dgScrapLot, SelectCartID);
                        }
                        else
                        {
                            if (dg.Name.Equals("dgDefectCtnr"))
                            {
                                Util.gridClear(dgDefectLot);
                            }
                            else
                            {
                                Util.gridClear(dgScrapLot);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 불량 대차 상세 조회
        /// </summary>
        private void SearchDefectDetail(C1DataGrid dg, string sCtnrID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CTNR_ID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CTNR_ID"] = sCtnrID;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_FORMATION_DEFECT_CART_LOT", "INDATA", "OUTDATA", dtRqst, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dg, bizResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 대차 Sheet 출력 자료
        /// </summary>
        /// <returns></returns>
        private DataTable PrintCartData(C1DataGrid dg)
        {
            try
            {
                int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dg, "CHK");
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CART_ID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CART_ID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIndex].DataItem, "CTNR_ID"));
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CART_SHEET_PC", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        /// <summary>
        ///  불량삭제
        /// </summary>
        private void DefectDelete()
        {
            #region 팝업으로 변경
            //try
            //{
            //    ShowLoadingIndicator();

            //    DataSet inData = new DataSet();

            //    //마스터 정보
            //    DataTable inDataTable = inData.Tables.Add("INDATA");
            //    inDataTable.Columns.Add("SRCTYPE", typeof(string));
            //    inDataTable.Columns.Add("IFMODE", typeof(string));
            //    inDataTable.Columns.Add("AREAID", typeof(string));
            //    inDataTable.Columns.Add("EQSGID", typeof(string));
            //    inDataTable.Columns.Add("USERID", typeof(string));
            //    inDataTable.Columns.Add("SHIFT", typeof(string));
            //    inDataTable.Columns.Add("WRK_USERID", typeof(string));
            //    inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));

            //    DataRow row = null;
            //    row = inDataTable.NewRow();
            //    row["SRCTYPE"] = "UI";
            //    row["IFMODE"] = "OFF";
            //    row["AREAID"] = LoginInfo.CFG_AREA_ID;
            //    row["EQSGID"] = LoginInfo.CFG_EQSG_ID;
            //    row["USERID"] = LoginInfo.USERID;
            //    row["SHIFT"] = txtShift.Tag.ToString();
            //    row["WRK_USERID"] = txtWorker.Tag.ToString();
            //    row["WRK_USER_NAME"] = txtWorker.Text.ToString();
            //    inDataTable.Rows.Add(row);

            //    //불량코드별 
            //    DataTable inResn = inData.Tables.Add("INRESN");
            //    inResn.Columns.Add("LOTID", typeof(string));

            //    for (int i = 0; i < dgDefectLot.Rows.Count; i++)
            //    {
            //        if (Util.NVC(DataTableConverter.GetValue(dgDefectLot.Rows[i].DataItem, "CHK")) == "1")
            //        {
            //            row = inResn.NewRow();
            //            row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDefectLot.Rows[i].DataItem, "INBOX_ID_DEF"));
            //            inResn.Rows.Add(row);
            //        }
            //    }

            //    try
            //    {
            //        new ClientProxy().ExecuteService_Multi("BR_PRD_CANCEL_DEFECT_LOT_PC", "INDATA,INRESN", null, (Result, bizException) =>
            //        {
            //            try
            //            {
            //                HiddenLoadingIndicator();

            //                if (bizException != null)
            //                {
            //                    Util.MessageException(bizException);
            //                    return;
            //                }

            //                Util.MessageInfo("SFU1275");//정상처리되었습니다.

            //                // 재조회
            //                SearchDefectCtnr(dgDefectCtnr);

            //                for (int i = 0; i < dgDefectCtnr.Rows.Count; i++)
            //                {
            //                    if (Util.NVC(DataTableConverter.GetValue(dgDefectCtnr.Rows[i].DataItem, "CTNR_ID")) == _selectDefectCart)
            //                    {
            //                        DataTableConverter.SetValue(dgDefectCtnr.Rows[i].DataItem, "CHK", 1);
            //                        dgDefectCtnr.SelectedIndex = i;
            //                    }
            //                }

            //                SearchDefectDetail(dgDefectLot, _selectDefectCart);
            //            }
            //            catch (Exception ex)
            //            {
            //                HiddenLoadingIndicator();
            //                Util.MessageException(ex);
            //            }
            //        }, inData);
            //    }
            //    catch (Exception ex)
            //    {
            //        HiddenLoadingIndicator();
            //        Util.MessageException(ex);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
            #endregion

            CMM_POLYMER_FORM_CART_DEFECT_DELETE popupDefectDelete = new CMM_POLYMER_FORM_CART_DEFECT_DELETE();
            popupDefectDelete.FrameOperation = this.FrameOperation;

            int idx_Ctnr = _Util.GetDataGridCheckFirstRowIndex(dgDefectCtnr, "CHK");
            int idx_Detail = _Util.GetDataGridCheckFirstRowIndex(dgDefectLot, "CHK");

            object[] parameters = new object[5];
            parameters[0] = Util.NVC(DataTableConverter.GetValue(dgDefectCtnr.Rows[idx_Ctnr].DataItem, "PROCID")).ToString(); //공정ID
            parameters[1] = Util.NVC(DataTableConverter.GetValue(dgDefectCtnr.Rows[idx_Ctnr].DataItem, "PROCNAME")).ToString(); //공정명
            parameters[2] = string.Empty; //설비 정보
            parameters[3] = Util.NVC(DataTableConverter.GetValue(dgDefectCtnr.Rows[idx_Ctnr].DataItem, "CTNR_ID")).ToString(); //대차ID

            DataTable dt = new DataTable();
            dt.Columns.Add("CTNR_ID", typeof(string));
            dt.Columns.Add("ASSY_LOTID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("RESNGRNAME", typeof(string));
            dt.Columns.Add("CAPA_GRD_CODE", typeof(string));
            dt.Columns.Add("CELL_QTY", typeof(decimal));
            dt.Columns.Add("WIPSEQ", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefectCtnr.Rows[idx_Ctnr].DataItem, "CTNR_ID")).ToString(); //대차ID
            dr["ASSY_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDefectLot.Rows[idx_Detail].DataItem, "LOTID_RT")).ToString();
            dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDefectLot.Rows[idx_Detail].DataItem, "INBOX_ID_DEF")).ToString();
            dr["RESNGRNAME"] = Util.NVC(DataTableConverter.GetValue(dgDefectLot.Rows[idx_Detail].DataItem, "DFCT_RSN_GR_NAME")).ToString();
            dr["CAPA_GRD_CODE"] = Util.NVC(DataTableConverter.GetValue(dgDefectLot.Rows[idx_Detail].DataItem, "CAPA_GRD_CODE")).ToString();
            dr["CELL_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefectLot.Rows[idx_Detail].DataItem, "WIPQTY")).ToString();
            dr["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgDefectLot.Rows[idx_Detail].DataItem, "WIPSEQ")).ToString();
            dt.Rows.Add(dr);

            parameters[4] = dr;

            popupDefectDelete.ShiftID = txtShift.Tag.ToString();
            popupDefectDelete.ShiftName = txtShift.Text.ToString();
            popupDefectDelete.WorkerID = txtWorker.Tag.ToString();
            popupDefectDelete.WorkerName = txtWorker.Text.ToString();

            C1WindowExtension.SetParameters(popupDefectDelete, parameters);

            popupDefectDelete.Closed += new EventHandler(popupDefectDelete_Closed);
            grdMain.Children.Add(popupDefectDelete);
            popupDefectDelete.BringToFront();
        }

        private void popupDefectDelete_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CART_DEFECT_DELETE popup = sender as CMM_POLYMER_FORM_CART_DEFECT_DELETE;

            if (popup.DialogResult == MessageBoxResult.OK)
            {
                // 재조회
                SearchDefectCtnr(dgDefectCtnr);

                for (int i = 0; i < dgDefectCtnr.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgDefectCtnr.Rows[i].DataItem, "CTNR_ID")) == _selectDefectCart)
                    {
                        DataTableConverter.SetValue(dgDefectCtnr.Rows[i].DataItem, "CHK", 1);
                        dgDefectCtnr.SelectedIndex = i;
                    }
                }

                SearchDefectDetail(dgDefectLot, _selectDefectCart);
            }

            this.grdMain.Children.Remove(popup);
        }

        #endregion

        #region [팝업]

        /// <summary>
        /// 불량대차등록 팝업
        /// </summary>
        private void popUpDefectCtnlInput()
        {
            COM001_221_DEFECT popupDefectCtnlInput = new COM001_221_DEFECT();
            popupDefectCtnlInput.FrameOperation = this.FrameOperation;
            popupDefectCtnlInput.DefectCtnr = "Y";
            popupDefectCtnlInput._PROCESS = cboProcess.SelectedValue.ToString();

            object[] parameters = new object[6];
            parameters[0] = Util.NVC(cboArea.SelectedValue);
            parameters[1] = Util.NVC(cboProcess.SelectedValue);
            parameters[2] = Util.NVC(cboEqsgid.SelectedValue);
            parameters[3] = txtShift.Tag.ToString();    //작업조 정보
            parameters[4] = txtWorker.Text.ToString();  //작업자명
            parameters[5] = txtWorker.Tag.ToString();   //작업자코드

            C1WindowExtension.SetParameters(popupDefectCtnlInput, parameters);
            popupDefectCtnlInput.Closed += new EventHandler(popupDefectCtnlInput_Closed);
            grdMain.Children.Add(popupDefectCtnlInput);
            popupDefectCtnlInput.BringToFront();
        }

        private void popupDefectCtnlInput_Closed(object sender, EventArgs e)
        {
            COM001_221_DEFECT popup = sender as COM001_221_DEFECT;

            if (popup.DialogResult == MessageBoxResult.OK)
            {
                // 재조회
                SearchDefectCtnr(dgDefectCtnr);
            }
            this.grdMain.Children.Remove(popup);
        }

        private void popUpDefectInput()
        {
            DataTable dtdefect = DataTableConverter.Convert(dgDefectCtnr.ItemsSource).Select("CHK = '1'").CopyToDataTable();

            COM001_221_DEFECT popupDefectInput = new COM001_221_DEFECT();
            popupDefectInput.DefectCtnr = "N";
            popupDefectInput._PROCESS = cboProcess.SelectedValue.ToString();
            popupDefectInput.FrameOperation = this.FrameOperation;

            object[] parameters = new object[7];
            parameters[0] = Util.NVC(cboArea.SelectedValue);
            parameters[1] = Util.NVC(cboProcess.SelectedValue);
            parameters[2] = Util.NVC(cboEqsgid.SelectedValue);
            parameters[3] = txtShift.Tag.ToString();    //작업조 정보
            parameters[4] = txtWorker.Text.ToString();  //작업자명
            parameters[5] = txtWorker.Tag.ToString();   //작업자코드
            parameters[6] = dtdefect;

            C1WindowExtension.SetParameters(popupDefectInput, parameters);
            popupDefectInput.Closed += new EventHandler(popupDefectInput_Closed);
            grdMain.Children.Add(popupDefectInput);
            popupDefectInput.BringToFront();
        }

        private void popupDefectInput_Closed(object sender, EventArgs e)
        {
            COM001_221_DEFECT popup = sender as COM001_221_DEFECT;

            if (popup.DialogResult == MessageBoxResult.OK)
            {
                // 재조회
                SearchDefectCtnr(dgDefectCtnr);
            }
            this.grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// 불량LOSS처리 팝업
        /// </summary>
        private void popUpDefectLoss()
        {
            CMM_POLYMER_FORM_CART_DEFECT_LOSS popupDefectLoss = new CMM_POLYMER_FORM_CART_DEFECT_LOSS();
            popupDefectLoss.FrameOperation = this.FrameOperation;

            int idx_Ctnr = _Util.GetDataGridCheckFirstRowIndex(dgDefectCtnr, "CHK");
            int idx_Detail = _Util.GetDataGridCheckFirstRowIndex(dgDefectLot, "CHK");

            object[] parameters = new object[5];
            parameters[0] = Util.NVC(DataTableConverter.GetValue(dgDefectCtnr.Rows[idx_Ctnr].DataItem, "PROCID")).ToString(); //공정ID
            parameters[1] = Util.NVC(DataTableConverter.GetValue(dgDefectCtnr.Rows[idx_Ctnr].DataItem, "PROCNAME")).ToString(); //공정명
            parameters[2] = string.Empty; //설비 정보
            parameters[3] = Util.NVC(DataTableConverter.GetValue(dgDefectCtnr.Rows[idx_Ctnr].DataItem, "CTNR_ID")).ToString(); //대차ID

            DataTable dt = new DataTable();
            dt.Columns.Add("CTNR_ID", typeof(string));
            dt.Columns.Add("ASSY_LOTID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("RESNGRNAME", typeof(string));
            dt.Columns.Add("CAPA_GRD_CODE", typeof(string));
            dt.Columns.Add("CELL_QTY", typeof(decimal));
            dt.Columns.Add("WIPSEQ", typeof(string));

            DataRow dr = dt.NewRow();
            dr["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefectCtnr.Rows[idx_Ctnr].DataItem, "CTNR_ID")).ToString(); //대차ID
            dr["ASSY_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDefectLot.Rows[idx_Detail].DataItem, "LOTID_RT")).ToString();
            dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgDefectLot.Rows[idx_Detail].DataItem, "INBOX_ID_DEF")).ToString();
            dr["RESNGRNAME"] = Util.NVC(DataTableConverter.GetValue(dgDefectLot.Rows[idx_Detail].DataItem, "DFCT_RSN_GR_NAME")).ToString();
            dr["CAPA_GRD_CODE"] = Util.NVC(DataTableConverter.GetValue(dgDefectLot.Rows[idx_Detail].DataItem, "CAPA_GRD_CODE")).ToString();
            dr["CELL_QTY"] = Util.NVC(DataTableConverter.GetValue(dgDefectLot.Rows[idx_Detail].DataItem, "WIPQTY")).ToString();
            dr["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgDefectLot.Rows[idx_Detail].DataItem, "WIPSEQ")).ToString();
            dt.Rows.Add(dr);

            parameters[4] = dr;

            C1WindowExtension.SetParameters(popupDefectLoss, parameters);

            popupDefectLoss.Closed += new EventHandler(popupDefectLoss_Closed);
            grdMain.Children.Add(popupDefectLoss);
            popupDefectLoss.BringToFront();
        }

        private void popupDefectLoss_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CART_DEFECT_LOSS popup = sender as CMM_POLYMER_FORM_CART_DEFECT_LOSS;

            if (popup.DialogResult == MessageBoxResult.OK)
            {
                // 재조회
                SearchDefectCtnr(dgDefectCtnr);
            }

            this.grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// 양품화등록 팝업
        /// </summary>
        private void popUpDefectGood()
        {
            CMM_POLYMER_FORM_CART_DEFECT_GOOD popupDefectGood = new CMM_POLYMER_FORM_CART_DEFECT_GOOD();
            popupDefectGood.FrameOperation = this.FrameOperation;

            object[] parameters = new object[3];
            parameters[0] = Util.NVC(cboArea.SelectedValue);
            parameters[1] = _Util.GetDataGridFirstRowBycheck(dgDefectCtnr, "CHK");
            parameters[2] = DataTableConverter.Convert(dgDefectLot.ItemsSource).Select();
            C1WindowExtension.SetParameters(popupDefectGood, parameters);

            popupDefectGood.Closed += new EventHandler(popupDefectGood_Closed);
            grdMain.Children.Add(popupDefectGood);
            popupDefectGood.BringToFront();
        }

        private void popupDefectGood_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CART_DEFECT_GOOD popup = sender as CMM_POLYMER_FORM_CART_DEFECT_GOOD;

            if (popup.DialogResult == MessageBoxResult.OK)
            {
                // 재조회
                SearchDefectCtnr(dgDefectCtnr);
            }

            this.grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// 폐기대차 등록 팝업
        /// </summary>
        private void popUpScrapCtnr()
        {
            CMM_POLYMER_FORM_CART_SCRAP popupScrap = new CMM_POLYMER_FORM_CART_SCRAP();
            popupScrap.FrameOperation = this.FrameOperation;

            object[] parameters = new object[6];
            parameters[0] = Util.NVC(cboArea.SelectedValue);
            parameters[1] = _Util.GetDataGridFirstRowBycheck(dgDefectCtnr, "CHK");
            parameters[2] = DataTableConverter.Convert(dgDefectLot.ItemsSource).Select();
            parameters[3] = txtShift.Tag.ToString();    //작업조 정보
            parameters[4] = txtWorker.Tag.ToString();   //작업자코드
            parameters[5] = txtWorker.Text.ToString();  //작업자명

            C1WindowExtension.SetParameters(popupScrap, parameters);

            popupScrap.Closed += new EventHandler(popupScrap_Closed);
            grdMain.Children.Add(popupScrap);
            popupScrap.BringToFront();
        }

        private void popupScrap_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CART_SCRAP popup = sender as CMM_POLYMER_FORM_CART_SCRAP;

            //if (popup.DialogResult == MessageBoxResult.OK)
            //{
            // 재조회
            SearchDefectCtnr(dgDefectCtnr);
            SearchDefectCtnr(dgScrapCtnr);
            //}

            this.grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// 폐기대상Cell등록 팝업
        /// </summary>
        private void popUpScrapCellInput()
        {
            DataTable dtscrap = DataTableConverter.Convert(dgScrapCtnr.ItemsSource).Select("CHK = '1'").CopyToDataTable();

            COM001_219_DEFECT_CELL_INPUT popupScrapCellInput = new COM001_219_DEFECT_CELL_INPUT();
            popupScrapCellInput.FrameOperation = this.FrameOperation;

            object[] parameters = new object[1];
            parameters[0] = dtscrap;

            C1WindowExtension.SetParameters(popupScrapCellInput, parameters);
            popupScrapCellInput.Closed += new EventHandler(popupScrapCellInput_Closed);
            grdMain.Children.Add(popupScrapCellInput);
            popupScrapCellInput.BringToFront();
        }

        private void popupScrapCellInput_Closed(object sender, EventArgs e)
        {
            COM001_219_DEFECT_CELL_INPUT popup = sender as COM001_219_DEFECT_CELL_INPUT;

            if (popup.DialogResult == MessageBoxResult.Cancel)
            {
                // 재조회
                SearchDefectCtnr(dgDefectCtnr);
            }

            this.grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// 대차시트 발행
        /// </summary>
        private void CartRePrint(DataRow[] printrow, int pageCnt)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupCartPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupCartPrint.FrameOperation = this.FrameOperation;
            popupCartPrint.DefectCartYN = "Y";
            popupCartPrint.PrintCount = pageCnt.ToString();
            popupCartPrint.DataRowCartSheet = printrow;
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgDefectCtnr, "CHK");

            object[] parameters = new object[5];
            parameters[0] = cboProcess.SelectedValue.ToString();
            parameters[1] = string.Empty;
            parameters[2] = Util.NVC(DataTableConverter.GetValue(dgDefectCtnr.Rows[rowIndex].DataItem, "CTNR_ID"));
            parameters[3] = "N";      // Direct 출력 여부
            parameters[4] = "N";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupCartPrint, parameters);

            popupCartPrint.Closed += new EventHandler(popupCartPrint_Closed);
            grdMain.Children.Add(popupCartPrint);
            popupCartPrint.BringToFront();
        }

        private void popupCartPrint_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_TAG_PRINT popup = sender as CMM_POLYMER_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }

            this.grdMain.Children.Remove(popup);
        }


        private void ScrapCartCancel()
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_CANCEL_SCRAP_CTNR_MCP";

                ShowLoadingIndicator();

                DataSet ds = new DataSet();
                DataTable indaTable = ds.Tables.Add("INDATA");
                indaTable.Columns.Add("SRCTYPE", typeof(string));
                indaTable.Columns.Add("IFMODE", typeof(string));
                indaTable.Columns.Add("USERID", typeof(string));

                DataRow row = indaTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["USERID"] = LoginInfo.USERID;
                indaTable.Rows.Add(row);

                DataTable inctnrTable = ds.Tables.Add("INCTNR");
                inctnrTable.Columns.Add("CTNR_ID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow item in dgScrapCtnr.Rows)
                {
                    if (item.Type != DataGridRowType.Item) continue;
                    if (Util.NVC(DataTableConverter.GetValue(item.DataItem, "CHK")) == "1")
                    {
                        DataRow dr = inctnrTable.NewRow();
                        dr["CTNR_ID"] = DataTableConverter.GetValue(item.DataItem, "CTNR_ID").GetString();
                        inctnrTable.Rows.Add(dr);
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INCTNR", null, (bizResult, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.MessageInfo("SFU1275");//정상처리되었습니다.
                    btnSearch_Click(btnSearch, null);

                }, ds);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 작업조, 작업자 팝업
        /// </summary>
        private void popUpShift()
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER2 wndPopup = new GMES.MES.CMM001.Popup.CMM_SHIFT_USER2();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = Util.NVC(cboArea.SelectedValue);
                Parameters[2] = Util.NVC(cboEqsgid.SelectedValue);
                Parameters[3] = Util.NVC(cboProcess.SelectedValue);
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = "";
                Parameters[7] = "N"; // 저장 플로그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_SHIFT_USER2 wndPopup = sender as CMM001.Popup.CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
            }
            this.grdMain.Children.Remove(wndPopup);
        }

        #endregion

        #region [Validation]

        /// <summary>
        /// 불량대차등록 Validation
        /// </summary>
        private bool ValidationDefectCtnlInput()
        {
            if (string.IsNullOrWhiteSpace(txtShift.Text))
            {
                // 작업조를 선택하세요.
                Util.MessageValidation("SFU1844");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtWorker.Text))
            {
                // 작업자를 선택 하세요.
                Util.MessageValidation("SFU1842"); 
                return false;
            }

            return true;
        }

        /// <summary>
        /// 불량추가 Validation
        /// </summary>
        private bool ValidationDefectInput()
        {
            if (dgDefectCtnr.Rows.Count == 1)
            {
                // 조회된 데이터가 없습니다.
                Util.MessageValidation("SFU3537");  
                return false;
            }

            DataRow[] drInfo = Util.gridGetChecked(ref dgDefectCtnr, "CHK");

            if (drInfo.Count() <= 0)
            {
                // 선택된 데이터가 없습니다.
                Util.MessageValidation("SFU3538");
                return false;
            }
            if (cboProcess.SelectedIndex == 0)
            {
                // 공정은 필수 입니다
                Util.MessageValidation("SFU1459");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtShift.Text))
            {
                // 작업조를 선택하세요.
                Util.MessageValidation("SFU1844");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtWorker.Text))
            {
                // 작업자를 선택 하세요.
                Util.MessageValidation("SFU1842"); 
                return false;
            }

            return true;
        }

        /// <summary>
        /// 불량삭제 Validation
        /// </summary>
        private bool ValidationDefectDelete()
        {
            if (dgDefectLot.Rows.Count == 1)
            {
                // 조회된 데이터가 없습니다.
                Util.MessageValidation("SFU3537");  
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dgDefectLot.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (dr.Length > 1)
            {
                // 한행만 선택 가능 합니다.
                Util.MessageValidation("SFU4023");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtShift.Text))
            {
                // 작업조를 선택하세요.
                Util.MessageValidation("SFU1844");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtWorker.Text))
            {
                // 작업자를 선택 하세요.
                Util.MessageValidation("SFU1842"); 
                return false;
            }

            return true;
        }

        /// <summary>
        /// 불량LOSS처리 Validation
        /// </summary>
        private bool ValidationDefectLoss()
        {
            if (dgDefectLot.Rows.Count == 1)
            {
                // 조회된 데이터가 없습니다.
                Util.MessageValidation("SFU1905"); 
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dgDefectLot.ItemsSource).Select("CHK = 1");

            if (dr.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (dr.Length > 1)
            {
                // 한행만 선택 가능 합니다.
                Util.MessageValidation("SFU4023");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 양품화 등록 Validation
        /// </summary>
        private bool ValidationDefectGood()
        {
            if (dgDefectCtnr.Rows.Count == 1)
            {
                // 조회된 데이터가 없습니다.
                Util.MessageValidation("SFU3537");  
                return false;
            }

            DataRow[] drInfo = Util.gridGetChecked(ref dgDefectCtnr, "CHK");

            if (drInfo.Count() <= 0)
            {
                // 선택된 데이터가 없습니다.
                Util.MessageValidation("SFU3538");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtShift.Text))
            {
                // 작업조를 선택하세요.
                Util.MessageValidation("SFU1844");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtWorker.Text))
            {
                // 작업자를 선택 하세요.
                Util.MessageValidation("SFU1842");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 폐기대차등록 
        /// </summary>
        private bool ValidationScrapCtnlInput()
        {
            if (dgDefectCtnr.Rows.Count == 1)
            {
                // 조회된 데이터가 없습니다.
                Util.MessageValidation("SFU3537");  
                return false;
            }

            DataRow[] drInfo = Util.gridGetChecked(ref dgDefectCtnr, "CHK");

            if (drInfo.Count() <= 0)
            {
                // 선택된 데이터가 없습니다.
                Util.MessageValidation("SFU3538");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtShift.Text))
            {
                // 작업조를 선택하세요.
                Util.MessageValidation("SFU1844");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtWorker.Text))
            {
                // 작업자를 선택 하세요.
                Util.MessageValidation("SFU1842");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 폐기대상Cell등록
        /// </summary>
        private bool ValidationScrapCellInput()
        {
            if (dgScrapCtnr.Rows.Count == 1)
            {
                // 조회된 데이터가 없습니다.
                Util.MessageValidation("SFU3537");  
                return false;
            }

            DataRow[] drInfo = Util.gridGetChecked(ref dgScrapCtnr, "CHK");

            if (drInfo.Count() <= 0)
            {
                // 선택된 데이터가 없습니다.
                Util.MessageValidation("SFU3538");
                return false;
            }

            return true;
        }

        private bool ValidationScrapCartCancel()
        {
            if (!CommonVerify.HasDataGridRow(dgScrapCtnr))
            {
                // 조회된 데이터가 없습니다.
                Util.MessageValidation("SFU3537");
                return false;
            }

            if (_Util.GetDataGridRowCountByCheck(dgScrapCtnr, "CHK") >= 1) return true;

            // 선택된 데이터가 없습니다.
            Util.MessageValidation("SFU3538");
            return false;
        }

        /// <summary>
        /// 폐기등록
        /// </summary>
        /// <returns></returns>
        private bool ValidationScrapInput()
        {
            // 조회된 데이터가 없습니다.
            if (dgScrapCtnr.Rows.Count == 1)
            {
                Util.MessageValidation("SFU3537");  
                return false;
            }

            DataRow[] drInfo = Util.gridGetChecked(ref dgScrapCtnr, "CHK");

            if (drInfo.Count() <= 0)
            {
                // 선택된 데이터가 없습니다.
                Util.MessageValidation("SFU3538");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 대차 시트 Validation
        /// </summary>
        private bool ValidationCartRePrint(C1DataGrid dg)
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dg, "CHK");

            if (rowIndex < 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            return true;
        }

        #endregion

        #region [Func]
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

        private void CartSheetPrint(C1DataGrid dg)
        {
            DataTable dt = PrintCartData(dg);

            if (dt == null || dt.Rows.Count == 0)
            {
                // 대차에 Inbox 정보가 없습니다.
                Util.MessageValidation("SFU4375");
                return;
            }
            // Page수 산출
            int PageCount = dt.Rows.Count % 40 != 0 ? (dt.Rows.Count / 40) + 1 : dt.Rows.Count / 40;
            int start = 0;
            int end = 0;
            DataRow[] dr;

            // Page 수만큼 Pallet List를 채운다
            for (int cnt = 0; cnt < PageCount; cnt++)
            {
                start = (cnt * 40) + 1;
                end = ((cnt + 1) * 40);

                dr = dt.Select("ROWNUM >=" + start + "And ROWNUM <=" + end);
                CartRePrint(dr, cnt + 1);
            }
        }


        #endregion


        #endregion



    }
}
