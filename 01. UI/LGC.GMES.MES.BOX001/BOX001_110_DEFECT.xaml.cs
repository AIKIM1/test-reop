/*************************************************************************************
 Created Date : 2017.09.08
      Creator : 
   Decription : 불량 등록 팝업
--------------------------------------------------------------------------------------
 [Change History]
 
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
using System.Windows.Controls;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_110_DEFECT : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        private string _procID = Process.SELECTING;       // 공정코드
        string _Shift = string.Empty;
        string _UserId = string.Empty;
        string _UserName = string.Empty;

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

        public BOX001_110_DEFECT()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length == 3)
            {
                _Shift = tmps[0] as string;
                _UserId = tmps[1] as string;
                _UserName = tmps[2] as string;
            }

            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: new C1ComboBox[] { cboLine });

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbParent: cboEquipmentSegmentParent);


            GetDefectInfo();
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

        private void dgDefect_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            //if (sender == null)
            //    return;

            //if (e.Cell.Row.Type.Equals(DataGridRowType.Top) || e.Cell.Row.Type.Equals(DataGridRowType.Bottom))
            //    return;

            //if (e.Cell.Column.Name.Equals("RESNQTY"))
            //{
            //    // 생산 Lot 실적 산출
            //    SetProductCalculator();
            //}
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
            try
            {
                // 변경전 자료 반영
                dgDefect.EndEditRow(true);

                // 불량정보를 저장 하시겠습니까?
                Util.MessageConfirm("SFU1587", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ConfirmProcess();
                    }
                });
            }

            catch
            { }
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
        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        #region [BizCall]      

        private void SetFcsEqsgID(C1ComboBox cbo, CommonCombo.ComboStatus cs, string areaid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PCSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = areaid;
                dr["PCSGID"] = "F";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ASSY_EQSGID", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "EQSGNAME";
                cbo.SelectedValuePath = "EQSGID";
                cbo.ItemsSource = AddStatus(dtResult, cs, "EQSGID", "EQSGNAME").Copy().AsDataView();

                if (!LoginInfo.CFG_EQSG_ID.Equals(""))
                {
                    cbo.SelectedValue = LoginInfo.CFG_EQSG_ID;
                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }

                    if (cbo.SelectedIndex < 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                }
                else
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 불량, 로스, 물청 정보 조회
        /// </summary>
        private void GetDefectInfo()
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;// cboArea.SelectedValue;
                newRow["PROCID"] = _procID;
                newRow["ACTID"] = "DEFECT_LOT";
             //   newRow["LOTID"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "LOTID");
            //    newRow["WIPSEQ"] = DataTableConverter.GetValue(dgLot.Rows[0].DataItem, "WIPSEQ");

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_GET_WIPRESONCOLLECT_INFO_ST", "INDATA", "OUTDATA", inTable);

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
                if (string.IsNullOrWhiteSpace(Util.NVC(cboArea.SelectedValue)) || Util.NVC(cboArea.SelectedValue) == "SELECT")
                {
                    //SFU3206	동을 선택해주세요
                    Util.MessageValidation("SFU3206");
                    return;
                }

                if (string.IsNullOrWhiteSpace(Util.NVC(cboLine.SelectedValue)) || Util.NVC(cboLine.SelectedValue) == "SELECT")
                {
                    //SFU1223 라인을 선택 하세요.
                    Util.MessageValidation("SFU1223");
                    return;
                }

                int idxPallet = _Util.GetDataGridCheckFirstRowIndex(dgLot, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                const string bizRuleName = "BR_PRD_REG_DEFECT_LOT_ST";

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE");
                inTable.Columns.Add("IFMODE");
                inTable.Columns.Add("AREAID");
                inTable.Columns.Add("EQSGID");
                inTable.Columns.Add("PKG_LOTID");
                inTable.Columns.Add("USERID");
                inTable.Columns.Add("SHIFT");
                inTable.Columns.Add("WRK_USERID");
                inTable.Columns.Add("WRK_USER_NAME");
                inTable.Columns.Add("POSTDATE");

                DataTable inResn = inDataSet.Tables.Add("INRESN");
                inResn.Columns.Add("WIP_DFCT_CODE", typeof(string));
                inResn.Columns.Add("WIPQTY", typeof(Decimal));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["AREAID"] = cboArea.SelectedValue;
                newRow["EQSGID"] = cboLine.SelectedValue;
                newRow["PKG_LOTID"] = DataTableConverter.GetValue(dgLot.Rows[idxPallet].DataItem, "LOTID");
                newRow["USERID"] = LoginInfo.USERID;
                newRow["SHIFT"] = _Shift;
                newRow["WRK_USERID"] = _UserId;
                newRow["WRK_USER_NAME"] = _UserName;
                newRow["POSTDATE"] = ldpDate.SelectedDateTime.ToString("yyyy-MM-dd");
                inTable.Rows.Add(newRow);

                foreach (C1.WPF.DataGrid.DataGridRow dRow in dgDefect.Rows)
                {
                    if (dRow.Type.Equals(DataGridRowType.Top) || dRow.Type.Equals(DataGridRowType.Bottom))
                        continue;

                    newRow = inResn.NewRow();
                    newRow["WIP_DFCT_CODE"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "RESNCODE"));
                    newRow["WIPQTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dRow.DataItem, "RESNQTY"));
                    inResn.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INRESN", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");

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
            }
        }
        #endregion

        #region[[Validation]
       
        #endregion

        #region [Func]
        

        #endregion

        #endregion

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                loadingIndicator.Visibility = Visibility.Visible;
                GetAssyLot();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            GetAssyLot();
        }

        private void GetAssyLot()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtLotID.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ASSY_LOT_ST", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult.Columns.Add("CHK");

                Util.GridSetData(dgLot, dtResult, FrameOperation, true);      

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

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
           // SetFcsEqsgID(cboLine, CommonCombo.ComboStatus.SELECT, Util.NVC(cboArea.SelectedValue));
        }

        private void dgLot_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //if (dgLot.CurrentRow == null || dgLot.SelectedIndex == -1)
            //{
            //    return;
            //}

            //try
            //{
            //    if (e.ChangedButton.ToString().Equals("Left") && dgLot.CurrentColumn.Name == "CHK")
            //    {
            //        string chkValue = Util.NVC(dgLot.GetCell(dgLot.CurrentRow.Index, dgLot.Columns["CHK"].Index).Value);

            //        //초기화
            //        dgDefect.ItemsSource = null;

            //        if (chkValue == "0")
            //        {
            //            DataTableConverter.SetValue(dgLot.Rows[dgLot.CurrentRow.Index].DataItem, "CHK", true);

            //            string sPalletid = Util.NVC(dgPalletInfo.GetCell(dgPalletInfo.CurrentRow.Index, dgPalletInfo.Columns["PALLETID"].Index).Value);
            //            string sLotType = Util.NVC(dgPalletInfo.GetCell(dgPalletInfo.CurrentRow.Index, dgPalletInfo.Columns["LOT_TYPE"].Index).Value);

            //            SelectTrayInfo(sPalletid);
            //            // 매거진 여부 확인 : N 이면 설비 / N 이 아니면 수작업
            //            //if (sLotType != "UI")
            //            //{
            //            //    // Tray 정보 조회 함수 호출
            //            //  //  SelectTrayInfo(sPalletid);
            //            //}
            //            //else
            //            //{
            //            //    // 수작업 Lot(매거진 Lot)에 해당하니 Tray 정보가 아닌 셀 정보만 존재
            //            //  //  SelectCellInfo(sPalletid, null);
            //            //}
            //        }
            //        else
            //            DataTableConverter.SetValue(dgPalletInfo.Rows[dgPalletInfo.CurrentRow.Index].DataItem, "CHK", false);

            //        SetSelectedQty();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
            //finally
            //{
            //    dgPalletInfo.CurrentRow = null;
            //}
        }

        private void dgLotChoice_Checked(object sender, RoutedEventArgs e)
        {
           if(sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            // Clear();

            if ((bool)rb.IsChecked && ((rb.DataContext as DataRowView).Row["CHK"].ToString().Equals(bool.FalseString) || string.IsNullOrWhiteSpace((rb.DataContext as DataRowView).Row["CHK"].ToString())))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

                //row 색 바꾸기
                dgLot.SelectedIndex = idx;

            }
        }
    }
}
