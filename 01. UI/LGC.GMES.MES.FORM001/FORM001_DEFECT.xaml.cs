/*************************************************************************************
 Created Date : 2017.09.08
      Creator : 
   Decription : 불량 등록 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2024.03.07  이병윤    : E20240110-000879 : 재공 불량 검출 위치 저장 추가[외관모드,검출위치]
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

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_DEFECT : C1Window, IWorkArea
    {
        #region Declaration

        public UcFormShift UcFormShift { get; set; }

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드

        private int PALLET_MAX_QUANTITY = 0;    //팔레트 최대 수량

        public bool ConfirmSave { get; set; }

        private bool _load = true;


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

        public FORM001_DEFECT()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();
                SetControlVisibility();

                SetGridVisulMode(); // 불량정보에 외관모드 콤보박스 추가
                SetGridDtctPstn();  // 검출위치에 외관모드 콤보박스 추가

                SetGridProduct();

                PalletMaxQuantitySetting();

                _load = false;
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
            parameters[8] = "FORM001_DEFECT";
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
                newRow["EQPTID"] = _eqptID;
                newRow["ACTID"] = "DEFECT_LOT";
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
                inResn.Columns.Add("VISUAL_MODE", typeof(string));
                inResn.Columns.Add("DTCT_PSTN", typeof(string));
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
                    newRow["DTCT_PSTN"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "DTCT_PSTN"));
                    newRow["VISUAL_MODE"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "VISUAL_MODE"));
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
            if(PALLET_MAX_QUANTITY > 0 && PALLET_MAX_QUANTITY < iDefectQty)
            {
                //불량 수량 %1 이 최대 수량 %2 보다 큽니다.
                Util.MessageValidation("SFU3799", new object[] { iDefectQty, PALLET_MAX_QUANTITY });
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




        #endregion

        #region 외관 모드
        private void SetGridVisulMode()
        {
            try
            {
                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("ATTR1", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["COM_TYPE_CODE"] = "DFCT_DTCT_PSTN";
                newRow["ATTR1"] = "VISUAL_MODE";

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_NAME", "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count < 1)
                {

                    dgDefect.Columns["VISUAL_MODE"].Visibility = Visibility.Collapsed;
                    return;
                }

                dgDefect.Columns["VISUAL_MODE"].Visibility = Visibility.Visible;

                (dgDefect.Columns["VISUAL_MODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult.Copy());

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region 검출위치
        private void SetGridDtctPstn()
        {
            try
            {
                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("ATTR2", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["COM_TYPE_CODE"] = "DFCT_DTCT_PSTN";
                newRow["ATTR2"] = "DTCT_PSTN";

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_NAME", "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count < 1)
                {

                    dgDefect.Columns["DTCT_PSTN"].Visibility = Visibility.Collapsed;
                    return;
                }

                dgDefect.Columns["DTCT_PSTN"].Visibility = Visibility.Visible;

                (dgDefect.Columns["DTCT_PSTN"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult.Copy());

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #endregion

    }
}
