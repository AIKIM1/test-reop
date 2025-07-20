/*************************************************************************************
 Created Date : 2020.03.23
      Creator : INS 김동일K
   Decription : 특별관리LOT 지정 화면
--------------------------------------------------------------------------------------
 [Change History]
  2020.03.23  INS 김동일K : Initial Created.
  2020.04.01  정문교      : CSR[C20200325-000359] > 전극창고(Pancake) 특별관리 LOT 지정 및 목적지 설비 추가
  2020.04.24  정문교      : CNB 조립인 경우 목적지 설비 숨김 처리
  2020.06.04  김동일      : [C20200409-000413] - [생산PI팀] 전극 특정 Slitter 호기 LOT에 대해 조립 특정 라인으로만 인계 기능 요청 건.
  2024.01.09  남재현      : STK 특별 TRAY 설정 추가 (Package 대기 특별 Tray 조회를 위한 Packaging 공정 추가.)
  2024.02.29  남재현      : STK 특별 TRAY 설정 추가 (Lot 지정 Tab : Packaging 공정 제외) 
  2024.04.02  안유수      E20240222-001642 라미 공정의 경우, 목적지 설비를 가져오는 로직 조건 중 ELTR_TYPE_CODE 컬럼 조건 제거
  2025.02.03  이민형      : 이상권 책임님 요청으로 전극동으로 로그인 한 경우  DA_BAS_SEL_EQUIPMENT_BY_ELECTRODETYPE_AREA_CBO -> DA_BAS_SEL_EQUIPMENT_BY_ELECTRODETYPE_AREA_CBO_ASSY
  2025.02.03  백상우      : [MES2.0] LOT지정/LOT해제탭에서 선택목록에서 선택된 Row만 저장되도록 수정 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_134.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_134 : UserControl, IWorkArea
    {
        private Util _util = new Util();
        private string _AreaType;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_134()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnSearchT1);
                listAuth.Add(btnSearchT2);
                listAuth.Add(btnSearchT3);
                listAuth.Add(btnSaveT1);
                listAuth.Add(btnSaveT2);

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                //dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
                //dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

                this.Loaded -= UserControl_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitCombo();
        }

        // 2024.02.29 남재현 : STK 특별 TRAY 설정 추가 (Lot 지정 Tab : Packaging 공정 제외) 
        private void C1TabControl_SelectionChanged(object sender, EventArgs e)
        {
            SetProcess();
        }

        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            _combo.SetCombo(cboAreaT1, CommonCombo.ComboStatus.SELECT, sCase: "cboArea");
            cboAreaT1.SelectedValue = LoginInfo.CFG_AREA_ID;
            
            _combo.SetCombo(cboAreaT2, CommonCombo.ComboStatus.SELECT, sCase: "cboArea");
            cboAreaT2.SelectedValue = LoginInfo.CFG_AREA_ID;
            
            _combo.SetCombo(cboAreaT3, CommonCombo.ComboStatus.SELECT, sCase: "cboArea");
            cboAreaT3.SelectedValue = LoginInfo.CFG_AREA_ID;

            SetAreaType();
            SetProcess();

            // CNB 자동자 조립, CNB ESS 인경우 목적지 설비 숨김
            if (LoginInfo.CFG_SHOP_ID == "G631" || LoginInfo.CFG_SHOP_ID == "G634")
            {
                grdTrgtInfo.Visibility = Visibility.Collapsed;
            }
            else
            {
                grdTrgtInfo.Visibility = Visibility.Visible;
            }

            cboAreaT1.SelectedValueChanged -= cboAreaT1_SelectedValueChanged;
            cboAreaT1.SelectedValueChanged += cboAreaT1_SelectedValueChanged;

            SetTrgtArea();
        }

        /// <summary>
        /// 동 Type > E : 전극, A : 조립
        /// </summary>
        private void SetAreaType()
        {
            try
            {
                _AreaType = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VW_AREA_TYPE_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _AreaType = dtResult.Rows[0]["AREA_TYPE_CODE"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 이동 설비 대상동
        /// </summary>
        private string SetMoveEqptArea()
        {
            try
            {
                if (_AreaType == "E")
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                    RQSTDT.Columns.Add("CMCODE", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CMCDTYPE"] = "RSV_EQPTID";
                    dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                    if (dtResult != null && dtResult.Rows.Count > 0)
                    {
                        return dtResult.Rows[0]["ATTRIBUTE1"].ToString();
                    }

                    return null;
                }
                else
                {
                    return LoginInfo.CFG_AREA_ID;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        /// <summary>
        /// 이동 설비
        /// </summary>
        private void SetMoveEquipment(int row)
        {
            try
            {
                int rowChkCount = DataTableConverter.Convert(dgListT1.ItemsSource).AsEnumerable().Count(r => r.Field<long>("CHK") == 1); // 2024.11.01. 김영국 - DataType문제로 변경. int -> long

                if (rowChkCount == 1)
                {
                    cboEquipmentT1.ItemsSource = null;

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("EQSGID", typeof(string));
                    RQSTDT.Columns.Add("PROCID", typeof(string));
                    RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = _AreaType == "E" ? Util.NVC(cboTrgtAreaT1.SelectedValue) : LoginInfo.CFG_AREA_ID;
                    // E7000 WAIT 또는 A5000 WAIT 시에는 노칭 설비 선택,  A5000 END 또는 A7000 WAIT 시에는 라미 설비 선택
                    dr["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgListT1.Rows[row].DataItem, "SELECT_TYPE")) == "1" ? Process.NOTCHING : Process.LAMINATION;
                    if (Util.NVC(DataTableConverter.GetValue(dgListT1.Rows[row].DataItem, "PROCID")) != Process.LAMINATION)
                    {
                        dr["ELTR_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgListT1.Rows[row].DataItem, "ELTR_TYPE_CODE"));
                    }
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = null;
                    if (_AreaType == "E")
                    {
                        dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_ELECTRODETYPE_AREA_CBO_ASSY", "RQSTDT", "RSLTDT", RQSTDT);                      
                    }
                    else
                    {
                        dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_ELECTRODETYPE_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                    }
                    
                    if (dtResult != null)
                    {
                        dr = dtResult.NewRow();
                        dr["CBO_NAME"] = "-SELECT-";
                        dr["CBO_CODE"] = "SELECT";
                        dtResult.Rows.InsertAt(dr, 0);

                        cboEquipmentT1.DisplayMemberPath = "CBO_NAME";
                        cboEquipmentT1.SelectedValuePath = "CBO_CODE";
                        cboEquipmentT1.ItemsSource = dtResult.Copy().AsDataView();
                        cboEquipmentT1.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetProcess()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null)
                {
                    DataView dvProcess = dtResult.DefaultView;
                    if (_AreaType == "E")
                    {
                        dvProcess.RowFilter = "CBO_CODE = '" + Process.ELEC_STORAGE + "'";
                    }
                    else
                    {
                        // 2024.02.29 남재현 : STK 특별 TRAY 설정 추가 (Lot 지정 Tab : Packaging 공정 제외) 
                        if (tiSelSpcl.IsSelected)
                        {
                            dvProcess.RowFilter = "CBO_CODE = '" + Process.NOTCHING + "' OR CBO_CODE = '" + Process.LAMINATION + "'";
                        }
                        else
                        {
                            // 2024.01.09 남재현 : STK 특별 TRAY 설정 추가 (Package 대기 특별 Tray 조회를 위한 Packaging 공정 추가.) 
                            dvProcess.RowFilter = "CBO_CODE = '" + Process.NOTCHING + "' OR CBO_CODE = '" + Process.LAMINATION + "' OR CBO_CODE = '" + Process.PACKAGING + "'";
                        }                       
                    }
                    DataTable dtProcess = dvProcess.ToTable();

                    dr = dtProcess.NewRow();
                    dr["CBO_NAME"] = "-SELECT-";
                    dr["CBO_CODE"] = "SELECT";
                    dtProcess.Rows.InsertAt(dr, 0);

                    cboProcessT1.DisplayMemberPath = "CBO_NAME";
                    cboProcessT1.SelectedValuePath = "CBO_CODE";
                    cboProcessT1.ItemsSource = dtProcess.Copy().AsDataView();
                    cboProcessT1.SelectedIndex = 0;

                    dtProcess.Rows.RemoveAt(0);

                    dr = dtProcess.NewRow();
                    dr["CBO_NAME"] = "-ALL-";
                    dr["CBO_CODE"] = "";
                    dtProcess.Rows.InsertAt(dr, 0);

                    cboProcessT2.DisplayMemberPath = "CBO_NAME";
                    cboProcessT2.SelectedValuePath = "CBO_CODE";
                    cboProcessT2.ItemsSource = dtProcess.Copy().AsDataView();
                    cboProcessT2.SelectedIndex = 0;

                    cboProcessT3.DisplayMemberPath = "CBO_NAME";
                    cboProcessT3.SelectedValuePath = "CBO_CODE";
                    cboProcessT3.ItemsSource = dtProcess.Copy().AsDataView();
                    cboProcessT3.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetSpclList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("PRJT_NAME", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));
                inDataTable.Columns.Add("SPCL_FLAG", typeof(string));
                
                DataRow newRow = inDataTable.NewRow();

                if (tiSelSpcl.IsSelected)
                {
                    Util.gridClear(dgListT1);
                    //Util.gridClear(dgSelListT1);

                    newRow["LANGID"] = LoginInfo.LANGID;

                    if (txtLotIdT1.Text.Trim().Length > 0)
                    {
                        newRow["LOTID"] = txtLotIdT1.Text;
                    }
                    else if (txtCstIDT1.Text.Trim().Length > 0)
                    {
                        newRow["CSTID"] = txtCstIDT1.Text;
                    }
                    else
                    {
                        string sAreaID = Util.GetCondition(cboAreaT1, "SFU3206");
                        if (sAreaID.Equals("")) return;

                        newRow["AREAID"] = sAreaID;
                        newRow["PROCID"] = Util.GetCondition(cboProcessT1, "SFU3207");
                        newRow["PRODID"] = txtProdIdT1.Text;
                        newRow["PRJT_NAME"] = txtPjtNameT1.Text;
                    }

                    newRow["SPCL_FLAG"] = "N";

                    inDataTable.Rows.Add(newRow);
                }
                else
                {
                    Util.gridClear(dgListT2);
                    //Util.gridClear(dgSelListT2);

                    newRow["LANGID"] = LoginInfo.LANGID;

                    if (txtLotIdT2.Text.Trim().Length > 0)
                    {
                        newRow["LOTID"] = txtLotIdT2.Text;
                    }
                    else if (txtCstIDT2.Text.Trim().Length > 0)
                    {
                        newRow["CSTID"] = txtCstIDT2.Text;
                    }
                    else
                    {
                        newRow["AREAID"] = Util.GetCondition(cboAreaT2, "SFU3206");
                        newRow["PROCID"] = Util.GetCondition(cboProcessT2, bAllNull: true);
                        newRow["PRODID"] = txtProdIdT2.Text;
                        newRow["PRJT_NAME"] = txtPjtNameT2.Text;
                    }

                    newRow["SPCL_FLAG"] = "Y";

                    inDataTable.Rows.Add(newRow);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_PRD_SEL_SPCL_LOT_LIST", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (tiSelSpcl.IsSelected)
                            Util.GridSetData(dgListT1, searchResult, FrameOperation, false);
                        else
                            Util.GridSetData(dgListT2, searchResult, FrameOperation, true);

                        txtLotIdT1.Text = "";
                        txtLotIdT2.Text = "";
                        txtCstIDT1.Text = "";
                        txtCstIDT2.Text = "";
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
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void SaveSpclLot(C1DataGrid dg, bool bSpcl, string sNote)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataSet inData = new DataSet();
                DataTable inTable = inData.Tables.Add("INDATA");
                inTable.Columns.Add("USERID", typeof(string));                
                inTable.Columns.Add("SPCL_FLAG", typeof(string));
                inTable.Columns.Add("SPCL_NOTE", typeof(string));
                inTable.Columns.Add("RSV_EQPTID", typeof(string));
                inTable.Columns.Add("RSV_EQSGID_LIST", typeof(string));

                DataTable inLotTable = inData.Tables.Add("INLOT");
                inLotTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["SPCL_FLAG"] = bSpcl ? "Y" : "N";
                newRow["SPCL_NOTE"] = sNote;

                if (tiSelSpcl.IsSelected)
                {
                    if (grdTrgtInfo.Visibility == Visibility.Visible)
                    {
                        if (cboTrgtEquiptmentSegmentT1.IsEnabled)
                        {
                            newRow["RSV_EQSGID_LIST"] = cboTrgtEquiptmentSegmentT1.SelectedItemsToString;
                            newRow["RSV_EQPTID"] = "";
                        }
                        else
                        {
                            newRow["RSV_EQSGID_LIST"] = "";
                            newRow["RSV_EQPTID"] = (cboEquipmentT1.SelectedValue == null || cboEquipmentT1.SelectedValue.ToString() == "SELECT") ? null : cboEquipmentT1.SelectedValue.ToString();
                        }
                    }
                    else
                    {
                        newRow["RSV_EQPTID"] = null;
                        newRow["RSV_EQSGID_LIST"] = null;
                    }
                }
                else
                {
                    newRow["RSV_EQPTID"] = "";
                    newRow["RSV_EQSGID_LIST"] = "";
                }
                
                inTable.Rows.Add(newRow);                

                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    newRow = inLotTable.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dg.Rows[i].DataItem, "LOTID"));
                    inLotTable.Rows.Add(newRow);
                }
                
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SPCL_LOT_NT_L", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");

                        GetSpclList();

                        Util.gridClear(dg);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Hidden;
                    }
                }, inData);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                loadingIndicator.Visibility = Visibility.Hidden;
            }
        }

        private void txtLotIdT1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                GetSpclList();
        }

        private void txtCstIDT1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                GetSpclList();
        }


        private void txtLotIdT1_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtLotIdT1 == null) return;
                InputMethod.SetPreferredImeConversionMode(txtLotIdT1, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCstIDT1_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCstIDT1 == null) return;
                InputMethod.SetPreferredImeConversionMode(txtCstIDT1, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearchT1_Click(object sender, RoutedEventArgs e)
        {
            GetSpclList();
        }

        private void btnSaveT1_Click(object sender, RoutedEventArgs e)
        {
            if (dgSelListT1.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1261");
                return;
            }
            else
            {
                DataTable dtInfo = DataTableConverter.Convert(dgSelListT1.ItemsSource);
                List<DataRow> drInfo = dtInfo.Select("CHK = 1")?.ToList();
                if (drInfo.Count == 0)
                {
                    Util.MessageValidation("SFU1261");
                    return;
                }
            }

            if (Util.NVC(txtRemarkT1.Text).Trim().Length < 1)
            {
                Util.MessageValidation("SFU1594");
                return;
            }

            if (_AreaType == "E" && cboEquipmentT1.IsEnabled && (cboEquipmentT1.SelectedValue == null || cboEquipmentT1.SelectedValue.ToString() == "SELECT"))
            {
                // 목적지 정보가 없습니다.
                Util.MessageValidation("SFU7024");
                return;
            }

            if (_AreaType == "E" && cboTrgtEquiptmentSegmentT1.IsEnabled && cboTrgtEquiptmentSegmentT1.SelectedItems.Count == 0)
            {
                // 목적지 정보가 없습니다.
                Util.MessageValidation("SFU7024");
                return;
            }

            Util.MessageConfirm("SFU1241", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    //선택된 Row만 저장되도록 수정
                    DataTable dtInfo = DataTableConverter.Convert(dgSelListT1.ItemsSource);
                    List<DataRow> drInfo = dtInfo.Select("CHK = 0")?.ToList();
                    foreach (DataRow dr in drInfo)
                    {
                        dtInfo.Rows.Remove(dr);
                    }
                    Util.GridSetData(dgSelListT1, dtInfo, FrameOperation, true);

                    SaveSpclLot(dgSelListT1, true, txtRemarkT1.Text);
                }
            });
        }

        private void dgListT1_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Type != DataGridRowType.Item) return;

            if (e.Column.Name.Equals("CHK"))
            {
                DataTable dtTgt = DataTableConverter.Convert(dgSelListT1.ItemsSource);

                // E7000 WAIT 또는 A5000 WAIT, A5000 END 또는 A7000 WAIT 같은 상태만 선택 가능
                if (dtTgt.Rows.Count > 0)
                {
                    if (dtTgt.Rows[0]["SELECT_TYPE"].ToString() != Util.NVC(DataTableConverter.GetValue(dgListT1.Rows[e.Row.Index].DataItem, "SELECT_TYPE")))
                    {
                        // 같은공정의 LOT이 아닙니다.
                        Util.MessageValidation("SFU2853");
                        e.Cancel = true;
                    }

                    // 전극 창고(E7000) 또는 A5000 WAIT인 경우 극성 체크
                    if (dtTgt.Rows[0]["SELECT_TYPE"].ToString() == "1")
                    {
                        if (dtTgt.Rows[0]["ELTR_TYPE_CODE"].ToString() != Util.NVC(DataTableConverter.GetValue(dgListT1.Rows[e.Row.Index].DataItem, "ELTR_TYPE_CODE")))
                        {
                            // 극성 정보가 다릅니다.
                            Util.MessageValidation("SFU2057");
                            e.Cancel = true;
                        }
                    }

                }
            }
        }

        private void dgListT1_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (e == null || e.Cell == null) return;

                if (Util.NVC(e.Cell.Column.Name).Equals("CHK"))
                {
                    if (e.Cell.Presenter != null &&
                        e.Cell.Presenter.Content != null &&
                        (e.Cell.Presenter.Content as CheckBox) != null &&
                        (e.Cell.Presenter.Content as CheckBox).IsChecked.HasValue &&
                        (bool)(e.Cell.Presenter.Content as CheckBox).IsChecked)
                    {
                        string sLot = Util.NVC(DataTableConverter.GetValue(dgListT1.Rows[e.Cell.Row.Index].DataItem, "LOTID"));

                        DataTable dtSrc = DataTableConverter.Convert(dgListT1.ItemsSource);
                        DataRow[] drSrc = DataTableConverter.Convert(dgListT1.ItemsSource).Select("LOTID ='" + sLot + "'");

                        if (drSrc == null || drSrc.Length < 1) return;

                        DataTable dtTgt = DataTableConverter.Convert(dgSelListT1.ItemsSource);
                        if (dtTgt == null || dtTgt.Rows.Count < 1)
                            dtTgt = dtSrc.Clone();

                        DataRow[] drTgt = dtTgt?.Select("LOTID ='" + sLot + "'");
                        if (drTgt != null && drTgt.Length > 0)
                        {
                            Util.MessageValidation("SFU2840");
                            return;
                        }

                        // MES 2.0 ItemArray 위치 오류 Patch
                        //dtTgt.Rows.Add(drSrc[0].ItemArray);
                        dtTgt.AddDataRow(drSrc[0]);

                        Util.GridSetData(dgSelListT1, dtTgt, FrameOperation, false);

                        // 목적지 설비 콤보 생성
                        SetMoveEquipment(e.Cell.Row.Index);
                    }
                    else
                    {
                        string sLot = Util.NVC(DataTableConverter.GetValue(dgListT1.Rows[e.Cell.Row.Index].DataItem, "LOTID"));

                        DataTable dtTgt = DataTableConverter.Convert(dgSelListT1.ItemsSource);
                        if (dtTgt == null || dtTgt.Rows.Count < 1) return;

                        DataRow[] drTgt = dtTgt?.Select("LOTID <> '" + sLot + "'");

                        if (dtTgt != null && drTgt.Length > 0)
                            Util.GridSetData(dgSelListT1, drTgt.CopyToDataTable(), FrameOperation, false);
                        else
                            Util.gridClear(dgSelListT1);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void txtLotIdT2_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                GetSpclList();
        }

        private void txtCstIDT2_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                GetSpclList();
        }
        
        private void txtLotIdT2_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtLotIdT2 == null) return;
                InputMethod.SetPreferredImeConversionMode(txtLotIdT2, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCstIDT2_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCstIDT2 == null) return;
                InputMethod.SetPreferredImeConversionMode(txtCstIDT2, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearchT2_Click(object sender, RoutedEventArgs e)
        {
            GetSpclList();
        }

        private void btnSaveT2_Click(object sender, RoutedEventArgs e)
        {
            if (dgSelListT2.GetRowCount() < 1)
            {
                Util.MessageValidation("SFU1261");
                return;
            }
            else
            {
                DataTable dtInfo = DataTableConverter.Convert(dgSelListT2.ItemsSource);
                List<DataRow> drInfo = dtInfo.Select("CHK = 1")?.ToList();
                if (drInfo.Count == 0)
                {
                    Util.MessageValidation("SFU1261");
                    return;
                }
            }

            if (Util.NVC(txtRemarkT2.Text).Trim().Length < 1)
            {
                Util.MessageValidation("SFU1594");
                return;
            }

            Util.MessageConfirm("SFU1241", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    //선택된 Row만 저장되도록 수정
                    DataTable dtInfo = DataTableConverter.Convert(dgSelListT2.ItemsSource);
                    List<DataRow> drInfo = dtInfo.Select("CHK = 0")?.ToList();
                    foreach (DataRow dr in drInfo)
                    {
                        dtInfo.Rows.Remove(dr);
                    }
                    Util.GridSetData(dgSelListT2, dtInfo, FrameOperation, true);

                    SaveSpclLot(dgSelListT2, false, txtRemarkT2.Text);
                }
            });
        }

        private void dgListT2_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e == null || e.Cell == null) return;

                if (Util.NVC(e.Cell.Column.Name).Equals("CHK"))
                {
                    if (e.Cell.Presenter != null &&
                        e.Cell.Presenter.Content != null &&
                        (e.Cell.Presenter.Content as CheckBox) != null &&
                        (e.Cell.Presenter.Content as CheckBox).IsChecked.HasValue &&
                        (bool)(e.Cell.Presenter.Content as CheckBox).IsChecked)
                    {
                        string sLot = Util.NVC(DataTableConverter.GetValue(dgListT2.Rows[e.Cell.Row.Index].DataItem, "LOTID"));

                        DataTable dtSrc = DataTableConverter.Convert(dgListT2.ItemsSource);
                        DataRow[] drSrc = DataTableConverter.Convert(dgListT2.ItemsSource).Select("LOTID ='" + sLot + "'");

                        if (drSrc == null || drSrc.Length < 1) return;

                        DataTable dtTgt = DataTableConverter.Convert(dgSelListT2.ItemsSource);
                        if (dtTgt == null || dtTgt.Rows.Count < 1)
                            dtTgt = dtSrc.Clone();

                        DataRow[] drTgt = dtTgt?.Select("LOTID ='" + sLot + "'");
                        if (drTgt != null && drTgt.Length > 0)
                        {
                            Util.MessageValidation("SFU2840");
                            return;
                        }

                        // MES 2.0 ItemArray 위치 오류 Patch
                        //dtTgt.Rows.Add(drSrc[0].ItemArray);
                        dtTgt.AddDataRow(drSrc[0]);

                        Util.GridSetData(dgSelListT2, dtTgt, FrameOperation, true);
                    }
                    else
                    {
                        string sLot = Util.NVC(DataTableConverter.GetValue(dgListT2.Rows[e.Cell.Row.Index].DataItem, "LOTID"));

                        DataTable dtTgt = DataTableConverter.Convert(dgSelListT2.ItemsSource);
                        if (dtTgt == null || dtTgt.Rows.Count < 1) return;

                        DataRow[] drTgt = dtTgt?.Select("LOTID <> '" + sLot + "'");

                        if (dtTgt != null && drTgt.Length > 0)
                            Util.GridSetData(dgSelListT2, drTgt.CopyToDataTable(), FrameOperation, true);
                        else
                            Util.gridClear(dgSelListT2);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLotIdT3_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                GetSpclHistList();
        }

        private void btnSearchT3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                GetSpclHistList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void txtLotIdT3_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtLotIdT3 == null) return;
                InputMethod.SetPreferredImeConversionMode(txtLotIdT3, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetSpclHistList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("PRJT_NAME", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("FROM", typeof(string));
                inDataTable.Columns.Add("TO", typeof(string));

                DataRow newRow = inDataTable.NewRow();

                Util.gridClear(dgListT3);

                newRow["LANGID"] = LoginInfo.LANGID;

                if (txtLotIdT3.Text.Trim().Length > 0)
                {
                    newRow["LOTID"] = txtLotIdT3.Text;
                }
                else
                {
                    newRow["AREAID"] = Util.GetCondition(cboAreaT3, "SFU3206");
                    newRow["PROCID"] = Util.GetCondition(cboProcessT3, bAllNull: true);
                    newRow["PRODID"] = txtProdIdT3.Text;
                    newRow["PRJT_NAME"] = txtPjtNameT3.Text;
                    newRow["FROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                    newRow["TO"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd");
                }

                inDataTable.Rows.Add(newRow);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_PRD_SEL_SPCL_LOT_LIST_HIST", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgListT3, searchResult, FrameOperation, true);

                        txtLotIdT3.Text = "";

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
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }


        private void SetTrgtArea()
        {
            try
            {
                if (_AreaType.Equals("E"))
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("SHOPID_FROM", typeof(string));
                    RQSTDT.Columns.Add("FROM_AREAID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SHOPID_FROM"] = LoginInfo.CFG_SHOP_ID;
                    dr["FROM_AREAID"] = Util.NVC(cboAreaT1.SelectedValue);
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_RELATION_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                    if (dtResult != null)
                    {
                        //dr = dtResult.NewRow();
                        //dr["CBO_NAME"] = "-SELECT-";
                        //dr["CBO_CODE"] = "SELECT";
                        //dtResult.Rows.InsertAt(dr, 0);

                        cboTrgtAreaT1.DisplayMemberPath = "CBO_NAME";
                        cboTrgtAreaT1.SelectedValuePath = "CBO_CODE";
                        cboTrgtAreaT1.ItemsSource = dtResult.Copy().AsDataView();
                        cboTrgtAreaT1.SelectedIndex = 0;
                    }
                }
                else
                {
                    DataTable dtTmp = ((DataView)cboAreaT1.ItemsSource).ToTable();

                    if (!dtTmp.Columns.Contains("LOGIS_TRF_TYPE_CODE"))
                        dtTmp.Columns.Add("LOGIS_TRF_TYPE_CODE", typeof(string));

                    for (int i = 0; i < dtTmp.Rows.Count; i++)
                    {
                        dtTmp.Rows[0]["LOGIS_TRF_TYPE_CODE"] = "AUTO";
                    }

                    cboTrgtAreaT1.DisplayMemberPath = "CBO_NAME";
                    cboTrgtAreaT1.SelectedValuePath = "CBO_CODE";
                    cboTrgtAreaT1.ItemsSource = dtTmp.Copy().AsDataView();
                    cboTrgtAreaT1.SelectedValue = LoginInfo.CFG_AREA_ID;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboAreaT1_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                SetTrgtArea();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboTrgtAreaT1_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboTrgtAreaT1 == null || cboTrgtAreaT1.ItemsSource == null) return;
                
                DataTable dtTmp = ((DataView)cboTrgtAreaT1.ItemsSource).ToTable();

                if (dtTmp == null) return;

                this.cboTrgtEquiptmentSegmentT1.ItemsSource = null;
                this.cboEquipmentT1.ItemsSource = null;

                if (Util.NVC(dtTmp.Rows[cboTrgtAreaT1.SelectedIndex]["LOGIS_TRF_TYPE_CODE"]).Equals("MANUAL"))  // 수동물류
                {
                    this.cboTrgtEquiptmentSegmentT1.IsEnabled = true;
                    //this.cboTrgtEquiptmentSegmentT1.SelectedIndex = 0;
                    this.cboEquipmentT1.IsEnabled = false;
                    this.cboEquipmentT1.SelectedIndex = 0;

                    cboTrgtEquiptmentSegmentT1.ApplyTemplate();
                    SetTrgtLine();
                }
                else
                {
                    this.cboTrgtEquiptmentSegmentT1.IsEnabled = false;
                    //this.cboTrgtEquiptmentSegmentT1.SelectedIndex = 0;
                    this.cboEquipmentT1.IsEnabled = true;
                    this.cboEquipmentT1.SelectedIndex = 0;

                    SetTrgtEquipment();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetTrgtEquipment()
        {
            try
            {
                cboEquipmentT1.ItemsSource = null;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboTrgtAreaT1.SelectedValue);
                // E7000 WAIT 또는 A5000 WAIT 시에는 노칭 설비 선택,  A5000 END 또는 A7000 WAIT 시에는 라미 설비 선택
                if (dgSelListT1.Rows.Count > 0)
                {
                    dr["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgSelListT1.Rows[0].DataItem, "SELECT_TYPE")) == "1" ? Process.NOTCHING : Process.LAMINATION;
                    if (Util.NVC(DataTableConverter.GetValue(dgSelListT1.Rows[0].DataItem, "PROCID")) != Process.LAMINATION)
                    {
                        dr["ELTR_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgSelListT1.Rows[0].DataItem, "ELTR_TYPE_CODE"));
                    }
                }
                else
                {
                    dr["PROCID"] = Process.NOTCHING;
                }
                
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = null;
                if (_AreaType == "E")
                {
                    dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_ELECTRODETYPE_AREA_CBO_ASSY", "RQSTDT", "RSLTDT", RQSTDT);
                }
                else
                {
                    dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_ELECTRODETYPE_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                }                

                if (dtResult != null)
                {
                    dr = dtResult.NewRow();
                    dr["CBO_NAME"] = "-SELECT-";
                    dr["CBO_CODE"] = "SELECT";
                    dtResult.Rows.InsertAt(dr, 0);

                    cboEquipmentT1.DisplayMemberPath = "CBO_NAME";
                    cboEquipmentT1.SelectedValuePath = "CBO_CODE";
                    cboEquipmentT1.ItemsSource = dtResult.Copy().AsDataView();
                    cboEquipmentT1.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetTrgtLine()
        {
            try
            {
                cboEquipmentT1.ItemsSource = null;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROD_GROUP", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Process.NOTCHING + "," + Process.LAMINATION;
                dr["AREAID"] = Util.NVC(cboTrgtAreaT1.SelectedValue);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_BY_PROCID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    cboTrgtEquiptmentSegmentT1.isAllUsed = false;
                    cboTrgtEquiptmentSegmentT1.DisplayMemberPath = "CBO_NAME";
                    cboTrgtEquiptmentSegmentT1.SelectedValuePath = "CBO_CODE";

                    cboTrgtEquiptmentSegmentT1.ItemsSource = DataTableConverter.Convert(dtResult);

                    //if (dtResult.Rows.Count == 1)
                    //{
                    //    cboTrgtEquiptmentSegmentT1.ItemsSource = DataTableConverter.Convert(dtResult);
                    //    cboTrgtEquiptmentSegmentT1.Check(-1);
                    //}
                    //else
                    //{
                    //    //cboTrgtEquiptmentSegmentT1.isAllUsed = true;
                    //    cboTrgtEquiptmentSegmentT1.ItemsSource = DataTableConverter.Convert(dtResult);
                    //    for (int i = 0; i < dtResult.Rows.Count; i++)
                    //    {
                    //        cboTrgtEquiptmentSegmentT1.Check(i);
                    //    }
                    //}
                }
                else
                {
                    cboTrgtEquiptmentSegmentT1.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


        }
    }
}
