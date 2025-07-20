/*************************************************************************************
 Created Date : 2023.05.24
      Creator : 
   Decription : RECHECK, NG LOT 불량 등록 팝업
--------------------------------------------------------------------------------------
 [Change History]
 2023.05.24  0.1   이홍주   SI               소형활성화 MES 복사
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid;
using System.Linq;
using System.Windows.Media;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001.Popup;
using System.Windows.Controls;
using System.Collections.Generic;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_RECHECK_NG : C1Window, IWorkArea
    {
        #region Declaration

        public UcFormShift UcFormShift { get; set; }

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();

        private bool IsLoading = true;
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;
        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드

        private int PALLET_MAX_QUANTITY = 0;    //팔레트 최대 수량

        public bool ConfirmSave { get; set; }

        //private bool _load = true;

        private int PALLET_DFCT_QTY = 0;
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public FCS002_RECHECK_NG()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsLoading)
            {

                InitializeUserControls();
                SetControl();     
                SetControlVisibility();
                SetGridProduct();  //생산실적

                InitCombo(); //2023.05.25

                PalletMaxQuantitySetting();

                IsLoading = false;
            }

        }

        private void PalletMaxQuantitySetting()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PALLET_MAX_QUANTITY";
                dr["CMCODE"] = LoginInfo.CFG_SHOP_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    PALLET_MAX_QUANTITY = int.Parse(Util.NVC(dtResult.Rows[0]["ATTRIBUTE1"]));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitializeUserControls()
        {
            UcFormShift = grdShift.Children[0] as UcFormShift;
        }

        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqptID = tmps[2] as string;

            // SET COMMON
            txtProcess.Text = tmps[1] as string;
            txtEquipment.Text = tmps[3] as string;

            // SET 생산 Lot 정보
            DataRow prodLot = tmps[4] as DataRow;

            if (prodLot == null)
                return;

            DataTable prodLotBind = new DataTable();

            prodLotBind = prodLot.Table.Clone();
            prodLotBind.ImportRow(prodLot);

            Util.GridSetData(dgLot, prodLotBind, null, true);

            GetDefectInfo();

            // Focus 
            dgDefect.Focus();
            dgDefect.LoadedCellPresenter -= dgDefect_LoadedCellPresenter;
            if (dgDefect.Rows.Count - dgDefect.FrozenBottomRowsCount > 0)
            {
                dgDefect.CurrentCell = dgDefect.GetCell(0, dgDefect.Columns["RESNQTY"].Index);
                dgDefect.Selection.Add(dgDefect.GetCell(0, dgDefect.Columns["RESNQTY"].Index));
            }
            dgDefect.LoadedCellPresenter += dgDefect_LoadedCellPresenter;

            UcFormShift = grdShift.Children[0] as UcFormShift;
            if (UcFormShift != null)
            {
                UcFormShift.ButtonShift.Click += ButtonShift_Click;
            }

            PALLET_DFCT_QTY = DataTableConverter.Convert(dgDefect.ItemsSource).AsEnumerable().Sum(r => r.Field<int>("RESNQTY"));
        }

        private void ButtonShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 popupShiftUser = new CMM_SHIFT_USER2 { FrameOperation = this.FrameOperation };

            object[] parameters = new object[9];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = LoginInfo.CFG_EQSG_ID;
            parameters[3] = _procID;
            parameters[4] = Util.NVC(UcFormShift.TextShift.Tag);
            parameters[5] = Util.NVC(UcFormShift.TextWorker.Tag);
            parameters[6] = _eqptID;
            parameters[7] = "N"; // 저장 Flag "Y" 일때만 저장.
            parameters[8] = "FCS002_RECHECK_NG";
            C1WindowExtension.SetParameters(popupShiftUser, parameters);

            popupShiftUser.Closed += new EventHandler(popupShiftUser_Closed);

            foreach (System.Windows.Controls.Grid tmp in Util.FindVisualChildren<System.Windows.Controls.Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupShiftUser);
                    popupShiftUser.BringToFront();
                    break;
                }
            }
        }

        private void popupShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 popup = sender as CMM_SHIFT_USER2;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                UcFormShift.TextShiftStartTime.Text = popup.WRKSTRTTIME;
                UcFormShift.TextShiftEndTime.Text = popup.WRKENDTTIME;
                UcFormShift.TextShiftDateTime.Text = UcFormShift.TextShiftStartTime.Text + " ~ " + UcFormShift.TextShiftEndTime.Text;

                //불량저장할때 작업자만 필요해서 여기만 값 확인 로직 넣음
                if (String.IsNullOrEmpty(popup.USERID))
                {
                    UcFormShift.TextWorker.Text = string.Empty;
                    UcFormShift.TextWorker.Tag = string.Empty;
                }
                else
                {
                    UcFormShift.TextWorker.Text = popup.USERNAME;
                    UcFormShift.TextWorker.Tag = popup.USERID;
                }

                UcFormShift.TextShift.Text = popup.SHIFTNAME;
                UcFormShift.TextShift.Tag = popup.SHIFTCODE;
            }

            this.Focus();
        }

        private void SetControlVisibility()
        {
            if (_procID.Equals(Process.SmallGrader))
            {
                dgProduct.Columns["DIFF_QTY"].Visibility = Visibility.Collapsed;
            }

            ////if (!_procID.Equals(Process.CircularCharacteristicGrader))
            ////{
            ////    dgLot.Columns["MKT_TYPE_NAME"].Visibility = Visibility.Collapsed;
            ////}
        }
        private void dgReCheckNGLotList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index + 1 - dataGrid.TopRows.Count > 0)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion

        #region 차이수량 Red 색상 처리
        private void dgProduct_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top) || e.Cell.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("DIFF_QTY"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                }
            }));

        }
        #endregion

        #region [불량수량 입력 - dgDefect_CommittedEdit, dgDefect_LoadedCellPresenter, dgDefect_PreviewKeyDown]
        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top) || e.Cell.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.IsReadOnly == true && e.Cell.Column.Visibility == Visibility.Visible)
                {
                    e.Cell.Presenter.Background = new System.Windows.Media.SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                }
            }));

        }

        private void dgDefect_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Type.Equals(DataGridRowType.Top) || e.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            if (e.Column.Name.Equals("RESNQTY"))
            {
                if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_QTY_CHG_BLOCK_FLAG")).Equals("Y"))
                {
                    e.Cancel = true;
                }
            }

        }

        private void dgDefect_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top) || e.Cell.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            if (e.Cell.Column.Name.Equals("RESNQTY"))
            {
                // 생산 Lot 실적 산출
                SetProductCalculator();
            }
        }

        private void dgDefect_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int rIdx = 0;
                int cIdx = 0;

                C1DataGrid grid = sender as C1DataGrid;

                rIdx = grid.CurrentCell.Row.Index;
                cIdx = grid.CurrentCell.Column.Index;

                if (grid.CurrentCell.Column.Name.Equals("RESNQTY"))
                {
                    if (grid.GetRowCount() > ++rIdx)
                    {
                        grid.Selection.Clear();
                        grid.CurrentCell = grid.GetCell(rIdx, cIdx);
                        grid.Selection.Add(grid.GetCell(rIdx, cIdx));

                        if (grid.GetRowCount() - 1 != rIdx)
                        {
                            grid.ScrollIntoView(rIdx + 1, cIdx);
                        }
                    }
                }
            }

        }

        #endregion

        #region [저장]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // 변경전 자료 반영
            try
            {
                dgDefect.EndEditRow(true);
            }
            catch
            { }

            if (!ValidateConfirmRun())
                return;

            // 불량정보를 저장 하시겠습니까?
            Util.MessageConfirm("SFU1587", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ConfirmProcess();
                }
            });
        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region [SEARCH]

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();

            //20230526 불량코드조회 하기 위해 추가
            GetDefectInfo();
        }

        #endregion

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //cboLine.SelectedValueChanged -= cboLine_SelectedValueChanged;
            SetEquipmentSegmentCombo(cboLine);
            //cboLine.SelectedValueChanged += cboLine_SelectedValueChanged;
        }

        private void cboLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboProcess.SelectedValueChanged -= cboProcess_SelectedValueChanged;

            // 공정 그룹
            SetProcessGroupCombo(cboProcGrpCode); //2021.04.06  KDH: Line별 공정그룹 Setting.
            SetProcessCombo(cboProcess);
            ///SetEquipmentCombo(cboEquipment);

            // Clear
            //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) START
            //txtProd.Text = string.Empty;
            txtPkgLotID.Text = string.Empty;
            ///txtGrpLotID.Text = string.Empty;
            ///txtSubLotID.Text = string.Empty;
            //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) END
            Util.gridClear(dgReCheckNGLotList);
            ///Util.gridClear(dgCellIDDetail);

            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
        }

        private void cboProcGrpCode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboProcess.SelectedValueChanged -= cboProcess_SelectedValueChanged;
            SetProcessCombo(cboProcess);
          
            // Clear
            txtPkgLotID.Text = string.Empty;
            Util.gridClear(dgReCheckNGLotList);  

            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            txtPkgLotID.Text = string.Empty; 
            Util.gridClear(dgReCheckNGLotList);          
        }

        private void txtPkgLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtPkgLotID.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
        }

        private void cboLotType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!IsLoading)
            {
                btnSearch_Click(null, null);
            }
        }


        #endregion

        #region User Method

        #region [BizCall]
        /// <summary>
        /// 생산 Lot 실적 조회
        /// </summary>
        private void SetGridProduct()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PR_LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PR_LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTID").GetString();
                newRow["WIPSEQ"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "WIPSEQ").GetString();
                newRow["PROCID"] = _procID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONFIRM_PRODUCT_SUM_FO", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgProduct, dtResult, null);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 불량, 로스, 물청 정보 조회
        /// </summary>
        private void GetDefectInfo()
        {
            try
            {
                DataTable inTable = _bizRule.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _procID;
                
                newRow["ACTID"] = "DEFECT_LOT";
                newRow["EQPTID"] = _eqptID;
                newRow["LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTID");     
                newRow["WIPSEQ"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "WIPSEQ");  

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPRESONCOLLECT_INFO_FORMATION", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgDefect, dtResult, null);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void ConfirmProcess()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_DFCT_PROD_LOT";

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("ACTUSER", typeof(string));

                DataTable inResn = inDataSet.Tables.Add("INRESN");
                inResn.Columns.Add("WIP_DFCT_CODE", typeof(string));
                inResn.Columns.Add("WIPQTY", typeof(Decimal));
                inResn.Columns.Add("ACTUSER", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_eqptID);
                newRow["PROD_LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTID");
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                foreach (C1.WPF.DataGrid.DataGridRow dRow in dgDefect.Rows)
                {
                    if (dRow.Type.Equals(DataGridRowType.Top) || dRow.Type.Equals(DataGridRowType.Bottom))
                        continue;

                    newRow = inResn.NewRow();
                    newRow["WIP_DFCT_CODE"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "RESNCODE"));
                    newRow["WIPQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dRow.DataItem, "RESNQTY"));
                    newRow["ACTUSER"] = UcFormShift.TextWorker.Tag;
                    inResn.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INRESN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);

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

        //FCS002_021_REL_JUDG 에서 복사한 참조용 Script
        //private void GetList()
        //{
        //    try
        //    {
        //        //Util.gridClear(dgCellData);

        //        DataTable dtRqst = new DataTable();
        //        dtRqst.TableName = "INDATA";
        //        dtRqst.Columns.Add("LANGID", typeof(string));
        //        dtRqst.Columns.Add("LOTID", typeof(string));
        //        dtRqst.Columns.Add("CSTID", typeof(string));
        //        dtRqst.Columns.Add("PROCID", typeof(string));

        //        DataRow dr = dtRqst.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["LOTID"] = "_sLotId";
        //        dr["CSTID"] = "_sTrayId";

        //        for (int i = 0; i < dgReCheckNGLotList.Rows.Count; i++)
        //        {
        //            if (Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[i].DataItem, "CHK")).Equals("True")
        //             || Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[i].DataItem, "CHK")).Equals("1"))
        //            {
        //                dr["PROCID"] += Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[i].DataItem, "PROCID")) + ",";
        //            }
        //        }
        //        dtRqst.Rows.Add(dr);

        //        ShowLoadingIndicator();
        //        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_LOAD_TRAY_INFO_RJUDG_MB", "INDATA", "OUTDATA", dtRqst);

        //        ///Util.GridSetData(dgCellData, dtRslt, FrameOperation, true);
        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    }
        //    finally
        //    {
        //        HiddenLoadingIndicator();
        //    }
        //}
        private void GetList()
        {
            try
            {
                Util.gridClear(dgReCheckNGLotList);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCGRID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("FROM_WIPDTTM_ST", typeof(string));
                inTable.Columns.Add("TO_WIPDTTM_ST", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = cboArea.GetBindValue();
                //상온, 고온, 출하 Aging 선택인 경우 무시.
                if (cboProcGrpCode.GetStringValue() == "3" || cboProcGrpCode.GetStringValue() == "4" || cboProcGrpCode.GetStringValue() == "7" || cboProcGrpCode.GetStringValue() == "9")
                {
                    newRow["EQSGID"] = null;
                }
                else
                {
                    newRow["EQSGID"] = Util.GetCondition(cboLine);
                }
                newRow["PROCGRID"] = cboProcGrpCode.GetBindValue();
                newRow["PROCID"] = cboProcess.GetBindValue();
                ///newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue).Equals(string.Empty) ? null : Util.GetCondition(cboEquipment);
                newRow["PROD_LOTID"] = txtPkgLotID.Text == string.Empty ? null : txtPkgLotID.Text;
                newRow["LOT_DETL_TYPE_CODE"] = Util.NVC(cboLotType.SelectedValue).Equals(string.Empty) ? null : Util.GetCondition(cboLotType);
                ///newRow["PRODID"] = txtProd.Text == string.Empty ? null : txtProd.Text;

                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_SEL_WIP_FCS_RECHECK_NG_DRB_MB", "RQSTDT", "RSLTDT", inTable, (result, Exception) =>
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
                            #region 선택된 로우 최상단으로 이동
                            //if (!string.IsNullOrEmpty(txtGrpLotID.Text.Trim()))
                            //{
                            //    List<DataRow> rows = result.AsEnumerable().Where(r => r["LOTID"].Equals(Util.NVC(txtGrpLotID.Text))).ToList();
                            //    if (rows.Count > 0)
                            //    {
                            //        rows[0]["CHK"] = true;
                            //        result.DefaultView.Sort = "CHK DESC";
                            //        result = result.DefaultView.ToTable();
                            //    }
                            //}
                            #endregion

                            Util.GridSetData(dgReCheckNGLotList, result, FrameOperation, true);
                            if (result.Rows.Count == 1)
                            {
                                string sLotID = Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[0].DataItem, "LOTID"));
                                dgReCheckNGLotList.SelectedIndex = 0;
                            }

                            //if (!string.IsNullOrEmpty(txtSubLotID.Text))
                            //{
                            //    txtSubLotID.SelectAll();
                            //    txtSubLotID.Focus();
                            //}
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
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
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            // 동
            ComCombo.SetCombo(cboArea, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ALLAREA");

            // 라인
            SetEquipmentSegmentCombo(cboLine);

            // 공정 그룹
            SetProcessGroupCombo(cboProcGrpCode); //2021.04.06  KDH: Line별 공정그룹 Setting.

            // 공정
            SetProcessCombo(cboProcess);

            // 설비 
            //SetEquipmentCombo(cboEquipment);

            // Lot 유형
            string[] sFilter1 = { "LOT_DETL_TYPE_CODE" };
            ComCombo.SetCombo(cboLotType, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "FORM_CMN", sFilter: sFilter1);


            //운영시점에 하위 값 점검할것 2023.05.26
            cboLotType.SelectedValue = "R";
            cboProcGrpCode.SelectedValue = "5"; //EOL Gr
            cboProcess.SelectedValue = "FF5101";//EOL
        }

        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMBO_FORM_LINE";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, cboArea.GetStringValue() };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo_Form_MB.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo_Form_MB.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }

        /// <summary>
        /// 공정그룹
        /// </summary>
        private void SetProcessGroupCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMBO_PROCESS_GROUP_BY_LINE";
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, cboArea.GetStringValue(), cboLine.SelectedValue == null ? null : cboLine.SelectedValue.ToString(), "PROC_GR_CODE" };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo_Form_MB.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo_Form_MB.ComboStatus.NONE, selectedValueText, displayMemberText, null);

        }

        /// <summary>
        /// 공정
        /// </summary>
        private void SetProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMBO_FORM_PROC_BY_LINE";
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "S26" };
            bool bAging = false;
            //상온, 고온, 출하 Aging 선택인 경우 무시.
            if (cboProcGrpCode.GetStringValue() == "3" || cboProcGrpCode.GetStringValue() == "4" || cboProcGrpCode.GetStringValue() == "7" || cboProcGrpCode.GetStringValue() == "9")
            {
                bAging = true;
            }
            else
            {
                bAging = false;
            }
            string[] arrCondition = { LoginInfo.LANGID, cboArea.GetStringValue(),
                                      cboLine.SelectedValue == null || bAging == true ? null : cboLine.SelectedValue.ToString(),
                                      cboProcGrpCode.SelectedValue == null ? null : Util.GetCondition(cboProcGrpCode) };

            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo_Form_MB.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo_Form_MB.ComboStatus.ALL, selectedValueText, displayMemberText, null);

            //2021.04.20 공통 Line에 대해 공정 List Setting되게 로직 추가 START
            DataTable dtcbo = DataTableConverter.Convert(cbo.ItemsSource);
            if (dtcbo == null || dtcbo.Rows.Count == 0)
            {
                const string bizRuleName1 = "DA_BAS_SEL_ALL_OP_CBO";
                string[] arrColumn1 = { "LANGID", "PROC_GR_CODE" };
                string[] arrCondition1 = { LoginInfo.LANGID, cboProcGrpCode.SelectedValue == null ? null : Util.GetCondition(cboProcGrpCode) };
                string selectedValueText1 = "CBO_CODE";
                string displayMemberText1 = "CBO_NAME";

                CommonCombo_Form_MB.CommonBaseCombo(bizRuleName1, cbo, arrColumn1, arrCondition1, CommonCombo_Form_MB.ComboStatus.NONE, selectedValueText1, displayMemberText1, null);
            }
            //2021.04.20 공통 Line에 대해 공정 List Setting되게 로직 추가 END
        }

        /// <summary>
        /// 설비
        /// </summary>
        //private void SetEquipmentCombo(C1ComboBox cbo)
        //{
        //    string saveEqptId = cboEquipment.GetStringValue();

        //    const string bizRuleName = "DA_BAS_SEL_COMBO_FORM_EQP_BY_PROC";
        //    string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "PROCGRID", "PROCID" };
        //    string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID,
        //        cboLine.SelectedValue == null ? null : cboLine.SelectedValue.ToString(),
        //        cboProcGrpCode.SelectedValue == null ? null : Util.GetCondition(cboProcGrpCode),
        //        cboProcess.SelectedValue == null ? null : cboProcess.SelectedValue.ToString() };

        //    cboEquipment.SetDataComboItem(bizRuleName, arrColumn, arrCondition, string.Empty, CommonCombo.ComboStatus.ALL, true, saveEqptId);
        //}

        // 공통함수로 뺄지 확인 필요 START
        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkReSetTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkReSetTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private DateTime GetJobDateTo(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkEndTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null).AddSeconds(-1);
            return dJobDate;
        }

        #endregion

        #region[[Validation]
        private bool ValidateConfirmRun()
        {
            if (dgLot.Rows.Count <= 0)
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            if (dgProduct.Rows.Count <= 0)
            {
                // 생산량이 없습니다.
                Util.MessageValidation("SFU1613");
                return false;
            }

            if (UcFormShift.TextWorker.Tag == null || string.IsNullOrEmpty(UcFormShift.TextWorker.Tag.ToString()))
            {
                // 작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return false;
            }

            DataTable dtDefect = DataTableConverter.Convert(dgDefect.ItemsSource);
            int iDefectQty = int.Parse(Util.NVC(dtDefect.Compute("SUM(RESNQTY)", string.Empty)));
            if (PALLET_MAX_QUANTITY > 0 && PALLET_MAX_QUANTITY < iDefectQty)
            {
                //불량 수량 %1 이 최대 수량 %2 보다 큽니다.
                Util.MessageValidation("SFU3799", new object[] { iDefectQty, PALLET_MAX_QUANTITY });
                return false;
            }

            if (txtNgCount.Text != txtSelCount.Text)
            {   //불량수량을 입력하세요. RECHECK수량만큼 ?
                Util.MessageValidation("PSS9140");
                return false;
            }
            
            return true;
        }
        #endregion

        #region [Func]

        private void SetProductCalculator()
        {
            int resnQtySum = DataTableConverter.Convert(dgDefect.ItemsSource).AsEnumerable().Sum(r => r.Field<int>("RESNQTY"));
            decimal inputQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgProduct.Rows[dgProduct.TopRows.Count].DataItem, "INPUT_QTY").ToString());
            decimal goodQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgProduct.Rows[dgProduct.TopRows.Count].DataItem, "GOOD_QTY").ToString());
            decimal dfctQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgProduct.Rows[dgProduct.TopRows.Count].DataItem, "DFCT_QTY").ToString());

            decimal calDfctQty = dfctQty - PALLET_DFCT_QTY + resnQtySum;

            DataTableConverter.SetValue(dgProduct.Rows[dgProduct.TopRows.Count].DataItem, "PRODUCT_QTY", (goodQty + calDfctQty));
            DataTableConverter.SetValue(dgProduct.Rows[dgProduct.TopRows.Count].DataItem, "DFCT_QTY", calDfctQty);
            DataTableConverter.SetValue(dgProduct.Rows[dgProduct.TopRows.Count].DataItem, "DIFF_QTY", inputQty - (goodQty + calDfctQty));

            txtNgCount.Text = calDfctQty.ToString();

            PALLET_DFCT_QTY = resnQtySum;
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

        private void dgReCheckNGLotChoice_Checked(object sender, RoutedEventArgs e)
        {

            int icount = 0;
            int iTemp = 0;
            int SumQty = 0;

            for (int i = 0; i < dgReCheckNGLotList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[i].DataItem, "CHK")).Equals("True")
                 || Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    //Int32.TryParse(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[i].DataItem, "WIP_CNT").ToString(), out iTemp);
                    SumQty = Util.NVC_Int(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[i].DataItem, "WIPQTY"));

                    icount = icount + SumQty;
                }

            }
            if(icount > 0)
            {
                txtSelCount.Text = icount.ToString();
            }
            else
            {
                txtSelCount.Text = "";
            }

        }

        #endregion

        #endregion



    }

}

