/*************************************************************************************
 Created Date : 2018.02.28
      Creator : 정문교
   Decription : 불량 대차 구성 완료
--------------------------------------------------------------------------------------
 [Change History]
    
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_POLYMER_FORM_CART_DEFECT_COMPLET.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_CART_DEFECT_COMPLET : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;        // 공정코드
        private string _procName = string.Empty;      // 공정명
        private string _eqptID = string.Empty;        // 설비코드
        private DataTable _inboxList;

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private bool _load = true;

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_POLYMER_FORM_CART_DEFECT_COMPLET()
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
                SetControlEnabled(false);
                SetCombo();
                _load = false;
            }

        }

        private void InitializeUserControls()
        {

        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _procName = tmps[1] as string;
            _eqptID = tmps[2] as string;

            txtProcess.Text = tmps[1] as string;
            txtCartID.Text = "NEW";

            DataRow[] DefectLot = tmps[4] as DataRow[];
            _inboxList = DefectLot.CopyToDataTable<DataRow>();

            SetGridCart();
            SetGridAssyLot();
            SetGridDefectGroupLot();
        }

        private void SetControlEnabled(bool Enabled)
        {
            btnCartRePrint.IsEnabled = Enabled;

            // 대차시트 발행버튼 클릭후 Enabled = true
            //grdCartMove.IsEnabled = Enabled;
            grdCartMove.IsEnabled = false;

            if (Enabled)
            {
                btnConfig.IsEnabled = false;
            }
        }

        private void SetCombo()
        {
            if (_load)
            {
                if (string.IsNullOrWhiteSpace(_eqptID))
                {
                    // 설비가 없는경우 포장 작업자
                    SetBoxingWorkerCombo();
                }
                else
                {
                    SetInspectorCombo();
                }
            }
            else
            {
                SetAreaCombo();
                SetMoveProcessCombo();
                SetMoveLineCombo();
            }
        }

        #endregion

        #region [불량대차구성]
        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationConfig())
                return;

            // 불량 대차를 생성하시겠습니까?
            Util.MessageConfirm("SFU4581", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DefectCartConfig();
                }
            });
        }
        #endregion

        #region 대차Sheet발행
        private void btnCartRePrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCartRePrint()) return;

            DataTable dt = PrintCartData();

            if (dt == null || dt.Rows.Count == 0)
            {
                // 대차에 Inbox 정보가 없습니다.
                Util.MessageValidation("SFU4375");
                return;
            }

            // Page수 산출
            int PageCount = dt.Rows.Count % 50 != 0 ? (dt.Rows.Count / 50) + 1 : dt.Rows.Count / 50;
            int start = 0;
            int end = 0;
            DataRow[] dr;

            // Page 수만큼 Pallet List를 채운다
            for (int cnt = 0; cnt < PageCount; cnt++)
            {
                start = (cnt * 50) + 1;
                end = ((cnt + 1) * 50);

                dr = dt.Select("ROWNUM >=" + start + "And ROWNUM <=" + end);

                // 대차Sheet발행
                CartRePrint(dr, cnt + 1);
            }
        }
        #endregion

        #region 조립Lot Check
        private void dgAssyLot_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgAssyLot.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (!cell.Column.Name.Equals("CHK"))
                {
                    return;
                }

                DataTable dt = new DataTable();
                dt = DataTableConverter.Convert(dgAssyLot.ItemsSource);

                if (dt.Rows[cell.Row.Index]["CHK"].Equals("True"))
                {
                    dt.Rows[cell.Row.Index]["CHK"] = false;
                }
                else
                {
                    dt.Rows[cell.Row.Index]["CHK"] = true;
                }

                dt.AcceptChanges();
                Util.GridSetData(dgAssyLot, dt, null);

                // 완성 Inbox 조회
                GetGridProductInboxList(dt);
            }

        }
        #endregion

        #region [공장 변경]
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
                return;

            SetMoveProcessCombo();
            SetMoveLineCombo();

        }
        #endregion

        #region [공정 변경]
        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString().Equals("SELECT"))
                return;

            SetMoveLineCombo();

        }
        #endregion

        #region [공장이동 체크]
        private void chkMoveArea_Checked(object sender, RoutedEventArgs e)
        {
            cboArea.IsEnabled = true;
            cboTakeOverUser.IsEnabled = true;
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
        }

        private void chkMoveArea_Unchecked(object sender, RoutedEventArgs e)
        {
            cboArea.IsEnabled = false;
            cboTakeOverUser.IsEnabled = false;
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
        }
        #endregion

        #region [이동]
        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationMove())
                return;

            // 이동 하시겠습니까?
            Util.MessageConfirm("SFU1763", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CartMove();
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

        #region Mehod

        private void SetAreaCombo()
        {
            const string bizRuleName = "DA_BAS_SEL_AREA_CBO";
            string[] arrColumn = { "LANGID", "SHOPID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID };
            string selectedValueText = cboArea.SelectedValuePath;
            string displayMemberText = cboArea.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboArea, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);

            cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
        }

        private void SetMoveProcessCombo()
        {
            try
            {
                if (dgCart.Rows.Count == 0)
                    return;

                // DATA Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("ROUTID", typeof(string));
                inTable.Columns.Add("FLOWID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = cboArea.SelectedValue ?? null;
                newRow["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "ROUTID"));
                newRow["FLOWID"] = Util.NVC(DataTableConverter.GetValue(dgCart.Rows[0].DataItem, "FLOWID"));
                newRow["PROCID"] = _procID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CART_MOVE_PROC_PC", "RQSTDT", "RSLTDT", inTable);

                //DataRow drSelect = dtResult.NewRow();
                //drSelect[cboProcess.SelectedValuePath] = "SELECT";
                //drSelect[cboProcess.DisplayMemberPath] = "- SELECT -";
                //dtResult.Rows.InsertAt(drSelect, 0);

                //cboProcess.ItemsSource = null;
                //cboProcess.ItemsSource = dtResult.Copy().AsDataView();
                //cboProcess.SelectedIndex = 0;

                //cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;

                cboProcess.ItemsSource = null;
                cboProcess.ItemsSource = dtResult.Copy().AsDataView();
                int index = 0;

                if (cboProcess.Items.Count > 1 || cboProcess.Items.Count == 0)
                {
                    DataTable dt = DataTableConverter.Convert(cboProcess.ItemsSource);
                    DataRow dr = dt.NewRow();
                    dr[cboProcess.SelectedValuePath] = "SELECT";
                    dr[cboProcess.DisplayMemberPath] = "- SELECT -";
                    dt.Rows.InsertAt(dr, 0);

                    cboProcess.ItemsSource = null;
                    cboProcess.ItemsSource = dt.Copy().AsDataView();
                }

                cboProcess.SelectedIndex = index;
                cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetMoveLineCombo()
        {
            try
            {
                string EqsgCode = string.Empty;

                if ((bool)chkMoveArea.IsChecked)
                {
                    EqsgCode = cboArea.SelectedValue == null ? null : cboArea.SelectedValue.ToString();
                }
                else
                {
                    EqsgCode = LoginInfo.CFG_AREA_ID;
                }

                const string bizRuleName = "DA_PRD_SEL_CART_MOVE_EQSG_PC";
                string[] arrColumn = { "LANGID", "SHOPID", "PROCID", "AREAID" };
                string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, Util.NVC(cboProcess.SelectedValue), EqsgCode };
                string selectedValueText = cboLine.SelectedValuePath;
                string displayMemberText = cboLine.DisplayMemberPath;

                CommonCombo.CommonBaseCombo(bizRuleName, cboLine, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, null);

                if (cboLine.Items.Count > 1 || cboLine.Items.Count == 0)
                {
                    DataTable dt = DataTableConverter.Convert(cboLine.ItemsSource);
                    DataRow dr = dt.NewRow();
                    dr[cboLine.SelectedValuePath] = "SELECT";
                    dr[cboLine.DisplayMemberPath] = "- SELECT -";
                    dt.Rows.InsertAt(dr, 0);

                    cboLine.ItemsSource = null;
                    cboLine.ItemsSource = dt.Copy().AsDataView();

                    int index = 0;
                    cboLine.SelectedIndex = index;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetInspectorCombo()
        {
            const string bizRuleName = "DA_PRD_SEL_INSPECTOR_PC";
            string[] arrColumn = { "LANGID", "EQPTID" };
            string[] arrCondition = { LoginInfo.LANGID, _eqptID };
            string selectedValueText = cboTakeOverUser.SelectedValuePath;
            string displayMemberText = cboTakeOverUser.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboTakeOverUser, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
            CommonCombo.CommonBaseCombo(bizRuleName, cboWorkUser, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
        }

        private void SetBoxingWorkerCombo()
        {
            const string bizRuleName = "DA_PRD_SEL_BOXING_WORKER_PC";
            string[] arrColumn = { "LANGID", "AREAID", "PROCID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, _procID };
            string selectedValueText = cboTakeOverUser.SelectedValuePath;
            string displayMemberText = cboTakeOverUser.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboTakeOverUser, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
            CommonCombo.CommonBaseCombo(bizRuleName, cboWorkUser, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
        }

        /// <summary>
        /// Cart List
        /// </summary>
        private void SetGridCart()
        {
            try
            {
                DataTable dt = new DataTable();

                foreach (C1.WPF.DataGrid.DataGridColumn col in dgCart.Columns)
                {
                    dt.Columns.Add(col.Name.ToString());
                }

                DataRow dr = dt.NewRow();
                dr["CTNR_ID"] = "NEW";
                dr["PRJT_NAME"] = Util.NVC(_inboxList.Rows[0]["PRJT_NAME"]);
                dr["PRODID"] = Util.NVC(_inboxList.Rows[0]["PRODID"]);
                dr["MKT_TYPE_NAME"] = Util.NVC(_inboxList.Rows[0]["MKT_TYPE_NAME"]);
                dr["WIP_QLTY_TYPE_NAME"] = Util.NVC(_inboxList.Rows[0]["WIP_QLTY_TYPE_NAME"]);
                dr["INBOX_COUNT"] = _inboxList.Rows.Count;
                dr["CELL_QTY"] = _inboxList.Select().AsEnumerable().Sum(r => r.Field<decimal>("CELL_QTY")).GetString();
                dt.Rows.Add(dr);

                Util.GridSetData(dgCart, dt, null, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Cart List(불량대차 구성 또는 대차 시트 출력후 조회)
        /// </summary>
        private void SetGridCartList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("CTNR_STAT_CODE", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CTNR_ID"] = txtCartID.Text;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_CTNR_PC", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgCart, dtResult, null, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 조립LOT List
        /// </summary>
        private void SetGridAssyLot()
        {
            try
            {
                var summarydata = from row in _inboxList.AsEnumerable()
                                  group row by new
                                  {
                                      AssyLot = row.Field<string>("ASSY_LOTID"),
                                      FormWorkType = row.Field<string>("FORM_WRK_TYPE_NAME"),
                                  } into grp
                                  select new
                                  {
                                      AssyLot = grp.Key.AssyLot,
                                      FormWorkType = grp.Key.FormWorkType,
                                      LotCount = grp.Count(),
                                      CellSum = grp.Sum(r => r.Field<decimal>("CELL_QTY"))
                                      
                                  };

                DataTable dt = new DataTable();
                foreach (C1.WPF.DataGrid.DataGridColumn col in dgAssyLot.Columns)
                {
                    dt.Columns.Add(col.Name.ToString());
                }

                foreach (var row in summarydata)
                {
                    DataRow dr = dt.NewRow();
                    dr["CHK"] = true;
                    dr["ASSY_LOTID"] = row.AssyLot;
                    dr["FORM_WRK_TYPE_NAME"] = row.FormWorkType;
                    dr["DEFECT_GROUP_LOTQTY"] = row.LotCount;
                    dr["CELL_QTY"] = row.CellSum;
                    dt.Rows.Add(dr);
                }

                Util.GridSetData(dgAssyLot, dt, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 불량그룹 LOT List
        /// </summary>
        private void SetGridDefectGroupLot()
        {
            try
            {
                // 순번 추가
                if (!_inboxList.Columns.Contains("SEQ"))
                {
                    _inboxList.Columns.Add("SEQ", typeof(int));
                }

                for (int row = 0; row <_inboxList.Rows.Count; row++)
                {
                    _inboxList.Rows[row]["SEQ"] = row + 1;
                }

                Util.GridSetData(dgDefectGroup, _inboxList, null, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetGridProductInboxList(DataTable dt)
        {
            DataTable dtinbox = _inboxList.Copy();
            DataRow[] drDel = DataTableConverter.Convert(dgAssyLot.ItemsSource).Select("CHK = false");

            foreach (DataRow rowdel in drDel)
            {
                dtinbox.Select("ASSY_LOTID = '" + Util.NVC(rowdel["ASSY_LOTID"]) + "'").ToList<DataRow>().ForEach(row => row.Delete());
            }
            dtinbox.AcceptChanges();

            Util.GridSetData(dgDefectGroup, dtinbox, FrameOperation, true);
        }

        /// <summary>
        /// 불량 대차 구성
        /// </summary>
        private void DefectCartConfig()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA SET
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataTable inCNTR = inDataSet.Tables.Add("INLOT");
                inCNTR.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROCID"] = _procID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(newRow);

                foreach (DataGridRow dRow in dgDefectGroup.Rows)
                {
                    if (dRow.Type == DataGridRowType.Item)
                    {
                        newRow = inCNTR.NewRow();
                        newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "LOTID"));
                        inCNTR.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_CTNR_DEFECT", "INDATA,INLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");
                        //this.DialogResult = MessageBoxResult.OK;

                        if (bizResult.Tables["OUTDATA"] != null && bizResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            txtCartID.Text = bizResult.Tables["OUTDATA"].Rows[0]["CTNR_ID"].ToString();

                            SetGridCartList();
                            SetControlEnabled(true);
                        }
                    }
                    catch (Exception ex)
                    {
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

        /// <summary>
        /// 대차 이동
        /// </summary>
        private void CartMove()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dt = DataTableConverter.Convert(cboProcess.ItemsSource);

                // DATA SET
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("TO_PROCID", typeof(string));
                inTable.Columns.Add("TO_ROUTID", typeof(string));
                inTable.Columns.Add("TO_FLOWID", typeof(string));
                inTable.Columns.Add("TO_EQSGID", typeof(string));
                inTable.Columns.Add("MOVE_USERID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("FROM_EQSGID", typeof(string));
                inTable.Columns.Add("FROM_PROCID", typeof(string));

                DataTable inCNTR = inDataSet.Tables.Add("INCTNR");
                inCNTR.Columns.Add("CTNR_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["TO_PROCID"] = cboProcess.SelectedValue.ToString();
                newRow["TO_ROUTID"] = dt.Rows[cboProcess.SelectedIndex]["ROUTID_TO"].ToString();
                newRow["TO_FLOWID"] = dt.Rows[cboProcess.SelectedIndex]["FLOWID_TO"].ToString();
                newRow["TO_EQSGID"] = cboLine.SelectedValue.ToString();
                newRow["MOVE_USERID"] = cboTakeOverUser.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["FROM_EQSGID"] = Util.NVC(_inboxList.Rows[0]["EQSGID"]);
                newRow["FROM_PROCID"] = Util.NVC(_inboxList.Rows[0]["PROCID"]);
                inTable.Rows.Add(newRow);

                newRow = inCNTR.NewRow();
                newRow["CTNR_ID"] = txtCartID.Text;
                inCNTR.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SEND_CTNR", "INDATA,INCTNR", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
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

        /// <summary>
        /// 대차 출력 자료
        /// </summary>
        private DataTable PrintCartData()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CART_ID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CART_ID"] = txtCartID.Text;
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CART_SHEET_PC", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }

        }

        #endregion

        #region [Func]

        private bool ValidationConfig()
        {
            if (dgDefectGroup.Rows.Count - dgDefectGroup.FrozenBottomRowsCount == 0)
            {
                // 불량정보가 없습니다.
                Util.MessageValidation("SFU1585");
                return false;
            }

            //if (_util.GetDataGridCheckFirstRowIndex(dgList, "CHK") < 0)
            //{
            //    //Util.Alert("선택된 항목이 없습니다.");
            //    Util.MessageValidation("SFU1651");
            //    return false;
            //}

            if (cboWorkUser.SelectedValue == null || cboWorkUser.SelectedValue.ToString().Equals("SELECT"))
            {
                // 작업자 정보를 입력하세요
                Util.MessageValidation("SFU4201");
                return false;
            }

            return true;
        }

        private bool ValidationCartRePrint()
        {
            if (dgCart == null || dgCart.Rows.Count == 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            return true;
        }

        private bool ValidationMove()
        {
            DataTable dt = DataTableConverter.Convert(dgCart.ItemsSource);
            DataRow[] dr = dt.Select("CTNR_ID = '" + txtCartID.Text + "'");

            if (dr.Length == 0)
            {
                // 대차 정보가 없습니다.
                Util.MessageValidation("SFU4365");
                return false;
            }

            if (Util.NVC_Int(dr[0]["CELL_QTY"]) == 0)
            {
                // 대차에 Inbox 정보가 없습니다.
                Util.MessageValidation("SFU4375");
                return false;
            }

            if (Util.NVC(dr[0]["CART_SHEET_PRT_FLAG"]).Equals("N"))
            {
                // 대차 발행후 이동 처리가 가능 합니다.
                Util.MessageValidation("SFU4406");
                return false;
            }

            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString().Equals("SELECT"))
            {
                // 대차 이동 공정을 선택하세요.
                Util.MessageValidation("SFU4362");
                return false;
            }

            if (cboLine.SelectedValue == null || cboLine.SelectedValue.ToString().Equals("SELECT"))
            {
                // 대차 이동 라인을 선택하세요.
                Util.MessageValidation("SFU4363");
                return false;
            }

            if (cboTakeOverUser.SelectedValue == null || cboTakeOverUser.SelectedValue.ToString().Equals("SELECT"))
            {
                // 인계자를 선택하세요.
                Util.MessageValidation("SFU4290");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 대차 Sheet 발행 팝업
        /// </summary>
        private void CartRePrint(DataRow[] printrow, int pageCnt)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupCartPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupCartPrint.FrameOperation = this.FrameOperation;

            popupCartPrint.PrintCount = pageCnt.ToString();
            popupCartPrint.DataRowCartSheet = printrow;
            popupCartPrint.DefectCartYN = "Y";

            object[] parameters = new object[5];
            parameters[0] = _procID;
            parameters[1] = _eqptID;
            parameters[2] = txtCartID.Text;
            parameters[3] = "N";      // Direct 출력 여부
            parameters[4] = "N";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupCartPrint, parameters);

            popupCartPrint.Closed += new EventHandler(popupCartPrint_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartPrint);
                    popupCartPrint.BringToFront();
                    break;
                }
            }
        }


        private void popupCartPrint_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_TAG_PRINT popup = sender as CMM_POLYMER_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }

            // 대차 이동 버튼 Enabled = true; 
            grdCartMove.IsEnabled = true;
            SetCombo();

            // 대차 츨력 여부 표시
            SetGridCartList();

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
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

        #endregion






    }
}