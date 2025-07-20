/*************************************************************************************
 Created Date : 2017.10.16
      Creator : 
   Decription : 외주 재튜빙 입출고
--------------------------------------------------------------------------------------
 [Change History]


 
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
using System.Windows.Threading;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_010 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
       

        public FORM001_010()
        {
            InitializeComponent();
            InitCombo();
            this.Loaded += UserControl_Loaded;
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

            #region 출고
            //동
            C1ComboBox[] cboAreaChildOut = { cboEquipmentSegmentOut };
            _combo.SetCombo(cboAreaOut, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaChildOut);

            //라인
            C1ComboBox[] cboEquipmentSegmentParentOut = { cboAreaOut };
            C1ComboBox[] cboEquipmentSegmentChildOut = { cboProcessOut };
            _combo.SetCombo(cboEquipmentSegmentOut, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentChildOut, cbParent: cboEquipmentSegmentParentOut);

            //공정
            C1ComboBox[] cbProcessParentOut = { cboEquipmentSegmentOut };
            String[] cbProcessChildOut = { "SEARCH", string.Empty, string.Empty, string.Empty };
            _combo.SetCombo(cboProcessOut, CommonCombo.ComboStatus.ALL, sFilter: cbProcessChildOut, sCase: "PROCESS_SORT", cbParent: cbProcessParentOut);

            SetShipToCombo(cboShipOut, cboAreaOut.SelectedValue.ToString(), false);
            cboAreaOut.SelectedValueChanged += cboAreaOut_SelectedValueChanged;

            #endregion

            #region 출고이력
            //동
            C1ComboBox[] cboAreaChildOutHis = { cboEquipmentSegmentOutHis };
            _combo.SetCombo(cboAreaOutHis, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaChildOutHis);

            //라인
            C1ComboBox[] cboEquipmentSegmentParentOutHis = { cboAreaOutHis };
            C1ComboBox[] cboEquipmentSegmentChildOutHis = { cboProcessOutHis };
            _combo.SetCombo(cboEquipmentSegmentOutHis, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentChildOutHis, cbParent: cboEquipmentSegmentParentOutHis);

            //공정
            C1ComboBox[] cbProcessParentOutHis = { cboEquipmentSegmentOutHis };
            String[] cbProcessChildOutHis = { "SEARCH", string.Empty, string.Empty, string.Empty };
            _combo.SetCombo(cboProcessOutHis, CommonCombo.ComboStatus.ALL, sFilter: cbProcessChildOutHis, sCase: "PROCESS_SORT", cbParent: cbProcessParentOutHis);

            #endregion

            #region 입고
            //동
            C1ComboBox[] cboAreaChildIn = { cboEquipmentSegmentIn };
            _combo.SetCombo(cboAreaIn, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaChildIn);

            //라인
            C1ComboBox[] cboEquipmentSegmentParentIn = { cboAreaIn };
            C1ComboBox[] cboEquipmentSegmentChildIn = { cboProcessIn };
            _combo.SetCombo(cboEquipmentSegmentIn, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentChildIn, cbParent: cboEquipmentSegmentParentIn);

            //공정
            C1ComboBox[] cbProcessParentIn = { cboEquipmentSegmentIn };
            String[] cbProcessChildIn = { "SEARCH", string.Empty, string.Empty, string.Empty };
            _combo.SetCombo(cboProcessIn, CommonCombo.ComboStatus.ALL, sFilter: cbProcessChildIn, sCase: "PROCESS_SORT", cbParent: cbProcessParentIn);

            SetShipToCombo(cboShipIn, cboAreaIn.SelectedValue.ToString(), true);
            cboAreaIn.SelectedValueChanged += cboAreaIn_SelectedValueChanged;

            #endregion

            #region 입고이력
            //동
            C1ComboBox[] cboAreaChildInHis = { cboEquipmentSegmentInHis };
            _combo.SetCombo(cboAreaInHis, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaChildInHis);

            //라인
            C1ComboBox[] cboEquipmentSegmentParentInHis = { cboAreaInHis };
            C1ComboBox[] cboEquipmentSegmentChildInHis = { cboProcessInHis };
            _combo.SetCombo(cboEquipmentSegmentInHis, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentChildInHis, cbParent: cboEquipmentSegmentParentInHis);

            //공정
            C1ComboBox[] cbProcessParentInHis = { cboEquipmentSegmentInHis };
            String[] cbProcessChildInHis = { "SEARCH", string.Empty, string.Empty, string.Empty };
            _combo.SetCombo(cboProcessInHis, CommonCombo.ComboStatus.ALL, sFilter: cbProcessChildInHis, sCase: "PROCESS_SORT", cbParent: cbProcessParentInHis);

            #endregion
        }

        #endregion

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSaveOut);
            listAuth.Add(btnSaveIn);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [출고]
        private void btnSearchOut_Click(object sender, RoutedEventArgs e)
        {
            GetLotListOut(true);
        }

        private void txtLotRTDOut_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtPalletIdOut_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        private void txtPalletIdOut_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    SetPalletSelectText(dgOut, dgOutProcess, txtPalletIdOut, cboShipOut);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void cboAreaOut_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (cboAreaOut.SelectedValue == null || cboAreaOut.SelectedValue.ToString().Equals("SELECT"))
            //    return;

            SetShipToCombo(cboShipOut, cboAreaOut.SelectedValue.ToString());
        }

        private void dgOut_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dgOut.GetRowCount() == 0)
            {
                return;
            }

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgOut.GetCellFromPoint(pnt);

            if (cell == null || cell.Value == null)
            {
                return;
            }
            if (cell.Column.Name != "CHK")
            {
                return;
            }

            SetPalletSelectChk(dgOut, dgOutProcess, null, cboShipOut);
        }

        private void btnSaveOut_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletOut())
                return;

            // 외주 재튜빙 출고처리를 하시겠습니까?
            Util.MessageConfirm("SFU4221", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    PalletOut();
                }
            });
        }
        #endregion

        #region [출고이력]
        private void btnSearchOutHis_Click(object sender, RoutedEventArgs e)
        {
            GetLotListOutHis();
        }

        private void txtLotRTDOutHis_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtPalletIdOutHis_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtPalletIdOutHis_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetLotListOutHis();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void dtpDateFromOut_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

            if (dtpDateFromOut.SelectedDateTime.Year > 1 && dtpDateToOut.SelectedDateTime.Year > 1)
            {
                //if ((dtpDateToOut.SelectedDateTime - dtpDateFromOut.SelectedDateTime).TotalDays > 7)
                //{
                //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                //    Util.MessageValidation("SFU2042", "7");

                //    dtpDateFromOut.SelectedDateTime = dtpDateToOut.SelectedDateTime.AddDays(-6);
                //    return;
                //}

                if ((dtpDateToOut.SelectedDateTime - dtpDateFromOut.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFromOut.SelectedDateTime = dtpDateToOut.SelectedDateTime;
                    return;
                }
            }
        }

        #endregion

        #region [입고]
        private void btnSearchIn_Click(object sender, RoutedEventArgs e)
        {
            GetLotListIn(true);
        }

        private void txtLotRTDIn_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtPalletIdIn_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtPalletIdIn_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    SetPalletSelectText(dgIn, dgInProcess, txtPalletIdIn);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void cboAreaIn_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (cboAreaIn.SelectedValue == null || cboAreaIn.SelectedValue.ToString().Equals("SELECT"))
            //    return;

            SetShipToCombo(cboShipIn, cboAreaIn.SelectedValue.ToString(), true);
        }

        private void dgIn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dgIn.GetRowCount() == 0)
            {
                return;
            }

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgIn.GetCellFromPoint(pnt);

            if (cell == null || cell.Value == null)
            {
                return;
            }
            if (cell.Column.Name != "CHK")
            {
                return;
            }

            SetPalletSelectChk(dgIn, dgInProcess);
        }

        private void btnSaveIn_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPalletIn())
                return;

            // 외주 재튜빙 입고처리를 하시겠습니까?
            Util.MessageConfirm("SFU4222", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    PalletIn();
                }
            });
        }
        #endregion

        #region [입고이력]
        private void btnSearchInHis_Click(object sender, RoutedEventArgs e)
        {
            GetLotListInHis();
        }

        private void txtLotRTDInHis_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtPalletIdInHis_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtPalletIdInHis_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetLotListInHis();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private void dtpDateFromIn_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

            if (dtpDateFromIn.SelectedDateTime.Year > 1 && dtpDateToIn.SelectedDateTime.Year > 1)
            {
                //if ((dtpDateToIn.SelectedDateTime - dtpDateFromIn.SelectedDateTime).TotalDays > 7)
                //{
                //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                //    Util.MessageValidation("SFU2042", "7");

                //    dtpDateFromIn.SelectedDateTime = dtpDateToIn.SelectedDateTime.AddDays(-6);
                //    return;
                //}

                if ((dtpDateToIn.SelectedDateTime - dtpDateFromIn.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFromIn.SelectedDateTime = dtpDateToIn.SelectedDateTime;
                    return;
                }
            }
        }

        #endregion

        #endregion

        #region Mehod

        #region [BizCall]
        /// <summary>
        /// 업체 콤보
        /// </summary>
        private void SetShipToCombo(C1ComboBox cb, string ShipTo, bool ComboAll = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ShipTo))
                {
                    cb.ItemsSource = null;
                    return;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = ShipTo;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETUBING_SHIPTO_CBO", "INDATA", "OUTDATA", inTable);

                DataRow newrow = dtResult.NewRow();
                if (ComboAll)
                {
                    newrow[cb.SelectedValuePath.ToString()] = "";
                    newrow[cb.DisplayMemberPath.ToString()] = "-ALL-";
                    dtResult.Rows.InsertAt(newrow, 0);
                }
                else
                {
                    if (dtResult != null && dtResult.Rows.Count > 1)
                    {
                        newrow[cb.SelectedValuePath.ToString()] = "SELECT";
                        newrow[cb.DisplayMemberPath.ToString()] = "-SELECT-";
                        dtResult.Rows.InsertAt(newrow, 0);
                    }
                }

                cb.ItemsSource = dtResult.Copy().AsDataView();
                cb.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region 출고
        /// <summary>
        /// Lot List 조회
        /// </summary>
        /// <param name="bButton"></param>
        private void GetLotListOut(bool bButton, bool bSave = false)
        {
            try
            {
                if (bButton == true)
                {
                    Util.gridClear(dgOut);
                    Util.gridClear(dgOutProcess);
                }

                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaOut, "SFU1499"); // 동을 선택하세요.
                if (dr["AREAID"].Equals(""))
                {
                    HiddenLoadingIndicator();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPalletIdOut.Text)) //PalletID 가 없는 경우
                {
                    dr["EQSGID"] = cboEquipmentSegmentOut.SelectedValue == null ? "" : cboEquipmentSegmentOut.SelectedValue.ToString();
                    dr["PROCID"] = cboProcessOut.SelectedValue == null ? "" : cboProcessOut.SelectedValue.ToString();
                    dr["PJT_NAME"] = txtPrjtNameOut.Text;
                    dr["PRODID"] = txtProdidOut.Text;
                    dr["LOTID_RT"] = txtLotRTDOut.Text;
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = txtPalletIdOut.Text;
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETUBING_SHIPTO_OUT", "INDATA", "OUTDATA", dtRqst);

                HiddenLoadingIndicator();

                if (dtRslt == null || dtRslt.Rows.Count == 0)
                {
                    if (bSave == false)
                    {
                        Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    }
                    return;
                }

                if (bButton == false)
                {
                    dtRslt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
                    DataTable dtSource = DataTableConverter.Convert(dgOut.ItemsSource);

                    if (dtSource == null || dtSource.Rows.Count == 0)
                    {
                        dtSource = dtRslt.Copy();
                    }
                    else
                    {
                        DataRow[] drSelect = dtSource.Select("PALLETID Like '" + txtPalletIdOut.Text + "'");
                        if (drSelect.Length == 0)
                        {
                            dtSource.Merge(dtRslt);
                        }
                    }

                    Util.GridSetData(dgOut, dtSource, FrameOperation, true);
                    SetPalletSelectChk(dgOut, dgOutProcess, txtPalletIdOut, cboShipOut);
                }
                else
                {
                    Util.GridSetData(dgOut, dtRslt, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void PalletOut()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtShipTo = DataTableConverter.Convert(cboShipOut.ItemsSource);

                DataSet inDataSet = new DataSet();

                //마스터 정보
                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("FROM_AREAID", typeof(string));
                inDataTable.Columns.Add("TO_SHOPID", typeof(string));
                inDataTable.Columns.Add("TO_SLOCID", typeof(string));
                inDataTable.Columns.Add("SHIPTO_ID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("WIPNOTE", typeof(string));

                DataRow newRow = null;
                newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["IFMODE"] = "OFF";
                newRow["FROM_AREAID"] = Util.NVC(DataTableConverter.GetValue(dgOutProcess.Rows[0].DataItem, "AREAID"));
                if (dtShipTo != null || dtShipTo.Rows.Count > 0)
                {
                    newRow["SHIPTO_ID"] = dtShipTo.Rows[cboShipOut.SelectedIndex]["CBO_CODE"].ToString();
                    newRow["TO_SHOPID"] = dtShipTo.Rows[cboShipOut.SelectedIndex]["SHOPID"].ToString();
                    newRow["TO_SLOCID"] = dtShipTo.Rows[cboShipOut.SelectedIndex]["TO_SLOC_ID"].ToString();
                }
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WIPNOTE"] = txtNoteOut.Text;
                inDataTable.Rows.Add(newRow);

                //LOT 정보
                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));

                newRow = null;
                foreach (C1.WPF.DataGrid.DataGridRow dRow in dgOutProcess.Rows)
                {
                    newRow = inLot.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "PALLETID"));
                    inLot.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SEND_OUT_RT", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.gridClear(dgOut);
                        Util.gridClear(dgOutProcess);

                        GetLotListOut(true, true);

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }

        #endregion

        #region 출고이력
        /// <summary>
        /// Lot List 조회
        /// </summary>
        /// <param name="bButton"></param>
        private void GetLotListOutHis()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("CHKINEXCEPT", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaOutHis, "SFU1499"); // 동을 선택하세요.
                if (dr["AREAID"].Equals(""))
                {
                    HiddenLoadingIndicator();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPrjtNameOutHis.Text)) //PalletID 가 없는 경우
                {
                    dr["FROM_DATE"] = Util.GetCondition(dtpDateFromOut);
                    dr["TO_DATE"] = Util.GetCondition(dtpDateToOut);
                    dr["EQSGID"] = cboEquipmentSegmentOutHis.SelectedValue == null ? "" : cboEquipmentSegmentOutHis.SelectedValue.ToString();
                    dr["PROCID"] = cboProcessOutHis.SelectedValue == null ? "" : cboProcessOutHis.SelectedValue.ToString();
                    dr["PJT_NAME"] = txtPrjtNameOutHis.Text;
                    dr["PRODID"] = txtProdidOutHis.Text;
                    dr["LOTID_RT"] = txtLotRTDOutHis.Text;
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = txtPrjtNameOutHis.Text;
                }

                dr["CHKINEXCEPT"] = (bool)chkInExcept.IsChecked ? "Y" : null;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETUBING_SHIPTO_OUT_HISTORY", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgOutHistory, dtRslt, FrameOperation, true);

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 입고
        /// <summary>
        /// Lot List 조회
        /// </summary>
        /// <param name="bButton"></param>
        public void GetLotListIn(bool bButton, bool bSave = false)
        {
            try
            {
                if (bButton == true)
                {
                    Util.gridClear(dgIn);
                    Util.gridClear(dgInProcess);
                }

                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("SHIPTO_ID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaIn, "SFU1499"); // 동을 선택하세요.
                if (dr["AREAID"].Equals(""))
                {
                    HiddenLoadingIndicator();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPalletIdIn.Text)) //PalletID 가 없는 경우
                {
                    dr["EQSGID"] = cboEquipmentSegmentIn.SelectedValue == null ? "" : cboEquipmentSegmentIn.SelectedValue.ToString();
                    dr["PROCID"] = cboProcessIn.SelectedValue == null ? "" : cboProcessIn.SelectedValue.ToString();
                    dr["SHIPTO_ID"] = cboShipIn.SelectedValue == null ? "" : cboShipIn.SelectedValue.ToString();
                    dr["PJT_NAME"] = txtPrjtNameIn.Text;
                    dr["PRODID"] = txtProdidIn.Text;
                    dr["LOTID_RT"] = txtLotRTDIn.Text;
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = txtPalletIdIn.Text;
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETUBING_SHIPTO_IN", "INDATA", "OUTDATA", dtRqst);

                HiddenLoadingIndicator();

                if (dtRslt == null || dtRslt.Rows.Count == 0)
                {
                    if (bSave == false)
                    {
                        Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    }
                    return;
                }

                if (bButton == false)
                {
                    dtRslt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
                    DataTable dtSource = DataTableConverter.Convert(dgIn.ItemsSource);

                    if (dtSource == null || dtSource.Rows.Count == 0)
                    {
                        dtSource = dtRslt.Copy();
                    }
                    else
                    {
                        DataRow[] drSelect = dtSource.Select("PALLETID Like '" + txtPalletIdIn.Text + "'");
                        if (drSelect.Length == 0)
                        {
                            dtSource.Merge(dtRslt);
                        }
                    }

                    Util.GridSetData(dgIn, dtSource, FrameOperation, true);
                    SetPalletSelectChk(dgIn, dgInProcess, txtPalletIdIn);
                }
                else
                {
                    Util.GridSetData(dgIn, dtRslt, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void PalletIn()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inDataSet = new DataSet();

                //마스터 정보
                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("TO_AREAID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("WIPNOTE", typeof(string));

                DataRow newRow = null;
                newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["IFMODE"] = "OFF";
                newRow["TO_AREAID"] = Util.NVC(DataTableConverter.GetValue(dgInProcess.Rows[0].DataItem, "AREAID"));
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WIPNOTE"] = txtNoteIn.Text;
                inDataTable.Rows.Add(newRow);

                //LOT 정보
                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("FROM_SHOPID", typeof(string));
                inLot.Columns.Add("FROM_SLOCID", typeof(string));
                inLot.Columns.Add("SHIPTO_ID", typeof(string));

                newRow = null;
                foreach (C1.WPF.DataGrid.DataGridRow dRow in dgInProcess.Rows)
                {
                    newRow = inLot.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "PALLETID"));
                    newRow["FROM_SHOPID"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "SHOPID"));
                    newRow["FROM_SLOCID"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "TO_SLOC_ID"));
                    newRow["SHIPTO_ID"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "SHIPTO_ID"));

                    inLot.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RECEIVE_OUT_RT", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.gridClear(dgIn);
                        Util.gridClear(dgInProcess);

                        GetLotListIn(true, true);

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }

        #endregion

        #region 입고 이력
        /// <summary>
        /// Lot List 조회
        /// </summary>
        /// <param name="bButton"></param>
        public void GetLotListInHis()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaInHis, "SFU1499"); // 동을 선택하세요.
                if (dr["AREAID"].Equals(""))
                {
                    HiddenLoadingIndicator();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPalletIdInHis.Text)) //PalletID 가 없는 경우
                {
                    dr["FROM_DATE"] = Util.GetCondition(dtpDateFromIn);
                    dr["TO_DATE"] = Util.GetCondition(dtpDateToIn);
                    dr["EQSGID"] = cboEquipmentSegmentInHis.SelectedValue == null ? "" : cboEquipmentSegmentInHis.SelectedValue.ToString();
                    dr["PROCID"] = cboProcessInHis.SelectedValue == null ? "" : cboProcessInHis.SelectedValue.ToString();
                    dr["PJT_NAME"] = txtPrjtNameInHis.Text;
                    dr["PRODID"] = txtProdidInHis.Text;
                    dr["LOTID_RT"] = txtLotRTDInHis.Text;
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = txtPalletIdInHis.Text;
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETUBING_SHIPTO_IN_HISTORY", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgInHistory, dtRslt, FrameOperation, true);

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region [Validation]

        private bool ValidationPalletOut()
        {
            if (dgOutProcess.Rows.Count == 0)
            {
                // 선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (cboShipOut.SelectedValue == null || cboShipOut.SelectedValue.ToString().Equals("SELECT"))
            {
                // 업체를 선택해 주세요.
                Util.MessageValidation("SFU4220");
                return false;
            }

            return true;
        }

        private bool ValidationPalletIn()
        {
            if (dgInProcess.Rows.Count == 0)
            {
                // 선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return false;
            }

            return true;
        }
        #endregion

        #region [Func]
        private void SetPalletSelectText(C1DataGrid dgFr, C1DataGrid dgTo, TextBox tx, C1ComboBox cb = null)
        {
            if (string.IsNullOrWhiteSpace(tx.Text))
            {
                return;
            }

            bool isList = false;

            if (dgFr.ItemsSource != null)
            {
                DataTable dt = DataTableConverter.Convert(dgFr.ItemsSource);
                DataRow[] dr = dt.Select("PALLETID = '" + tx.Text + "'");

                if (dr.Length > 0)
                {
                    // 조회 목록에 있다
                    isList = true;
                    int idx = dt.Rows.IndexOf(dr[0]);

                    dt.Rows[idx]["CHK"] = true;
                    DataTableConverter.SetValue(dgFr.Rows[idx].DataItem, "CHK", true);
                    dgFr.SelectedIndex = idx;
                    dgFr.ScrollIntoView(idx, dgFr.Columns["CHK"].Index);

                    DataTable dtInsert = new DataTable();
                    DataRow[] drInsert = dt.Select("CHK = 1");
                    dtInsert = drInsert.CopyToDataTable<DataRow>();

                    Util.GridSetData(dgTo, dtInsert, null, true);

                    tx.Focus();
                    tx.Text = string.Empty;

                    //// 출하처
                    //if (cb != null)
                    //{
                    //    SetShipToCombo(cb, Util.NVC(dr[0]["AREAID"].ToString()));
                    //}
                }
            }

            if ((bool)isList == false)
            {
                if (cb == null)
                {
                    // 입고
                    GetLotListIn(false);
                }
                else
                {
                    // 출고
                    GetLotListOut(false);
                }
            }

        }

        private void SetPalletSelectChk(C1DataGrid dgFr, C1DataGrid dgTo, TextBox tx = null, C1ComboBox cb = null)
        {
            // 그리드에서 체크 버튼 클릭 또는 검색 조건에서 Pallet Key In
            DataTable dt = DataTableConverter.Convert(dgFr.ItemsSource);

            DataRow[] dr = dt.Select("CHK = 1");

            if (dr.Length > 0)
            {
                DataTable dtInsert = new DataTable();
                dtInsert = dr.CopyToDataTable<DataRow>();

                Util.GridSetData(dgTo, dtInsert, null, true);

                //// 출하처
                //if (cb != null)
                //{
                //    SetShipToCombo(cb, Util.NVC(dr[0]["AREAID"].ToString()));
                //}
            }
            else
            {
                Util.gridClear(dgTo);
            }

            if (tx != null)
            {
                dgFr.SelectedIndex = dgFr.Rows.Count - 1;
                dgFr.ScrollIntoView(dgFr.Rows.Count - 1, dgFr.Columns["CHK"].Index);

                tx.Focus();
                tx.Text = string.Empty;
            }

        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }



        #endregion

        #endregion

    }
}
