/*************************************************************************************
 Created Date : 2018.02.28
      Creator : 정문교
   Decription : 불량 대차 구성
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
using System.Threading;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_POLYMER_FORM_CART_DEFECT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_POLYMER_FORM_CART_DEFECT : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _procID = string.Empty;        // 공정코드
        private string _procName = string.Empty;      // 공정명
        private string _lineID = string.Empty;        // 라인코드
        private string _eqptID = string.Empty;        // 설비코드

        private DataTable _defectList;
        private string _defectLotID;

        private CheckBoxHeaderType _inBoxHeaderType;

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

        public CMM_POLYMER_FORM_CART_DEFECT()
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
            _lineID = tmps[4] as string;

            txtProcess.Text = tmps[1] as string;
            txtEquipment.Text = tmps[3] as string;
            txtEquipmentSegment.Text = tmps[5] as string;

            _inBoxHeaderType = CheckBoxHeaderType.Zero;

            txtDefectTag.Focus();
        }
        private void SetCombo()
        {
            // 시장유형
            CommonCombo _combo = new CommonCombo();
            string[] sFilter = { "MKT_TYPE_CODE" };
            _combo.SetCombo(cboMKTType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            // 라인
            cboEquipmentSegment.ApplyTemplate();
            SetEquipmentSegmentCombo();

            SetDefectGroupCombo();
        }

        #endregion

        private void ctbDefectCart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (btnConfig == null) return;

            if (((System.Windows.FrameworkElement)ctbDefectCart.SelectedItem).Name.Equals("DefectCart"))
            {
                //btnConfig.IsEnabled = true;
                btnCellInsert.Visibility = Visibility.Collapsed;

                Dispatcher.BeginInvoke((ThreadStart)delegate
                {
                    txtDefectTag.Focusable = true;
                    txtDefectTag.Focus();
                });
            }
            else
            {
                //btnConfig.IsEnabled = false;
                btnCellInsert.Visibility = Visibility.Visible;
            }
        }

        #region 불량대차구성 탭
        private void txtDefectTag_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ValidationScan())
                    txtDefectTag.Text = string.Empty;
                else
                    txtDefectTag.SelectAll();
            }

        }
        #endregion

        #region 불량LOT 재공 탭
        /// <summary>
        /// 같은제품, 같은 시장유형만 선택
        /// </summary>
        private void dgList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Type.Equals(DataGridRowType.Top) || e.Row.Type.Equals(DataGridRowType.Bottom))
                return;

            if (e.Column.Name.Equals("CHK"))
            {
                DataTable dt = new DataTable();
                DataRow[] dr = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = 1");

                if (dr.Length == 0)
                {
                    return;
                }

                dt = dr.CopyToDataTable<DataRow>();

                DataRow[] drSelect = dt.Select("PRODID = '" + Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "PRODID")) + "' And MKT_TYPE_CODE = '" + Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "MKT_TYPE_CODE")) + "'");

                if (drSelect.Length == 0)
                {
                    e.Cancel = true;
                }
            }

        }

        /// <summary>
        /// 조회조건 Enter시 바로 조회
        /// </summary>
        private void txtPjtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationSearch())
                    return;

                DefectSearch();
            }
        }

        private void txtProdID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationSearch())
                    return;

                DefectSearch();
            }
        }

        private void txtAssyLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ValidationSearch())
                    return;

                DefectSearch();
            }
        }

        /// <summary>
        /// 불량대차조회
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            DefectSearch();
        }

        /// <summary>
        /// Cell 등록
        /// </summary>
        private void btnCellInsert_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCellInsert(dgList))
                return;

            CellInsert();
        }

        #endregion

        #region [스캔대차삭제]
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDelete(dgDefect))
                return;

            // 저장안된 row 삭제
            GridRowDelete();

        }
        #endregion

        #region [불량대차구성]
        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            if (((System.Windows.FrameworkElement)ctbDefectCart.SelectedItem).Name.Equals("DefectCart"))
            {
                if (!ValidationConfigScan(dgDefect))
                    return;
            }
            else
            {
                if (!ValidationConfigList(dgList))
                    return;
            }

            DefectCartComplet();
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

        #region 불량대차구성 탭
        /// <summary>
        /// Scan 뷸량 태그 조회
        /// </summary>
        private DataTable DefectTagSearch()
        {
            try
            {
                // 스캔값 Split
                string[] ScanValue = txtDefectTag.Text.Split('+');

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("RESNGR_ABBR_CODE", typeof(string));
                inTable.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = ScanValue.Length > 0 ? ScanValue[0] : null;
                newRow["RESNGR_ABBR_CODE"] = ScanValue.Length > 1 ? ScanValue[1] : null;
                newRow["CAPA_GRD_CODE"] = ScanValue.Length > 2 ? ScanValue[2] : null;
                newRow["PROCID"] = _procID;
                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_DEFECT_GROUP_SCAN", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        #endregion

        #region 불량LOT 재공 탭
        private void SetDefectGroupCombo()
        {
            const string bizRuleName = "DA_QCA_SEL_DEFECT_GROUP_PC";
            string[] arrColumn = { "LANGID", "AREAID", "PROCID", "EQPTID", "ACTID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, _procID, _eqptID, "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT" };
            string selectedValueText = cboMKTType.SelectedValuePath;
            string displayMemberText = cboMKTType.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cboDefectGroup, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, null);
        }

        private void SetEquipmentSegmentCombo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = _procID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_PC", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    cboEquipmentSegment.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        cboEquipmentSegment.ItemsSource = DataTableConverter.Convert(dtResult);
                        cboEquipmentSegment.Check(-1);
                    }
                    else
                    {
                        cboEquipmentSegment.isAllUsed = true;
                        cboEquipmentSegment.ItemsSource = DataTableConverter.Convert(dtResult);
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            cboEquipmentSegment.Check(i);
                        }
                    }
                }
                else
                {
                    cboEquipmentSegment.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 불량조회
        /// </summary>
        private void DefectSearch()
        {
            try
            {
                ShowLoadingIndicator();

                _defectList = new DataTable();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                inTable.Columns.Add("ASSY_LOTID", typeof(string));
                inTable.Columns.Add("DFCT_RSN_GR_ID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedItemsToString);
                newRow["PROCID"] = _procID;
                newRow["PRJT_NAME"] = Util.NVC(txtPjtName.Text);
                newRow["PRODID"] = Util.NVC(txtProdID.Text);
                newRow["MKT_TYPE_CODE"] = cboMKTType.SelectedValue == null ? "" : cboMKTType.SelectedValue.ToString();
                newRow["ASSY_LOTID"] = Util.NVC(txtAssyLotID.Text);
                newRow["DFCT_RSN_GR_ID"] = cboDefectGroup.SelectedValue == null ? "" : cboDefectGroup.SelectedValue.ToString();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_QCA_SEL_DEFECT_GROUP_LIST", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgList, bizResult, FrameOperation, true);

                        // 제품, 시장유형 Group Table 생성
                        _defectList = bizResult.DefaultView.ToTable(true, "PRODID", "MKT_TYPE_CODE");
                        _defectList.Columns.Add("CHKYN", typeof(string));
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
        /// Cell 등록 권한
        /// </summary>
        private bool ChkCellAuthority()
        {
            bool IsProc = false;

            try
            {
                // FORM_INSP_PROCID(최종외관), FORM_REWORK_PROCID((양품화)

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AUTHID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["AUTHID"] = "ASSYMF_CELLID_REG";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_USER_AUTH_PC", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    IsProc = true;
                }

                return IsProc;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return IsProc;
            }
        }
        #endregion

        #endregion

        #region [Func]

        private bool ValidationSearch()
        {
            return true;
        }

        private bool ValidationCellInsert(C1DataGrid dg)
        {
            if (dg.Rows.Count - dg.FrozenBottomRowsCount <= 0)
            {
                // 조회된 Data가 없습니다.
                Util.MessageValidation("SFU1905");
                return false;
            }

            DataRow[] dr = DataTableConverter.Convert(dg.ItemsSource).Select("CHK = 1");

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

            if (!ChkCellAuthority())
            {
                // USER[%1]는 권한[%2]을 가지고 있지 않습니다.
                Util.MessageValidation("SFU3520", LoginInfo.USERID, ObjectDic.Instance.GetObjectName("Cell 등록"));
                return false;
            }

            _defectLotID = Util.NVC(dr[0]["LOTID"]);

            return true;
        }

        private bool ValidationScan()
        {
            if (string.IsNullOrWhiteSpace(txtDefectTag.Text))
            {
                // LOT ID 를 입력 또는 스캔하세요.
                Util.MessageValidation("SFU2836");
                return false;
            }

            // Scan Data 존재여부 체크
            DataTable dt = DataTableConverter.Convert(dgDefect.ItemsSource);
            DataRow[] dr;

            if (dt != null && dt.Rows.Count > 0)
            {
                dr = dt.Select("DEFECT_TAG = '" + txtDefectTag.Text + "'");
            }
            else
            {
                dr = null;
            }

            if (dr != null && dr.Length > 0)
            {
                // 존재하면 체크 표시
                int rowindex = dt.Rows.IndexOf(dr[0]);
                DataTableConverter.SetValue(dgDefect.Rows[rowindex].DataItem, "CHK", true);
            }
            else
            {
                DataTable dtScan = DefectTagSearch();

                if (dtScan == null || dtScan.Rows.Count == 0)
                {
                    // 스캔한 데이터가 없습니다.
                    Util.MessageValidation("SFU2060");
                    return false;
                }

                if (dt == null || dt.Rows.Count == 0)
                {
                    // 스캔 Data 등록
                    dt = dtScan.Copy();
                }
                else
                {
                    // 같은 제품, 시장유형 체크
                    if (Util.NVC(dt.Rows[0]["PRODID"]) != Util.NVC(dtScan.Rows[0]["PRODID"]))
                    {
                        // 동일 제품이 아닙니다.
                        Util.MessageValidation("SFU1502");
                        return false;
                    }

                    if (Util.NVC(dt.Rows[0]["MKT_TYPE_CODE"]) != Util.NVC(dtScan.Rows[0]["MKT_TYPE_CODE"]))
                    {
                        // 동일한 시장유형이 아닙니다.
                        Util.MessageValidation("SFU4271");
                        return false;
                    }

                    dt.Merge(dtScan);
                }

                Util.GridSetData(dgDefect, dt, FrameOperation, true);
            }

            return true;
        }

        private bool ValidationDelete(C1DataGrid dg)
        {
            DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

            if (dt == null || dt.Rows.Count == 0)
            {
                // 선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return false;
            }

            DataRow[] dr = dt.Select("CHK = 1");

            if (dr.Length == 0)
            {
                // 선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return false;
            }

            return true;
        }

        private bool ValidationConfigScan(C1DataGrid dg)
        {
            if (dg.Rows.Count - dg.FrozenBottomRowsCount == 0)
            {
                // 불량정보가 없습니다.
                Util.MessageValidation("SFU1585");
                return false;
            }

            return true;
        }

        private bool ValidationConfigList(C1DataGrid dg)
        {
            if (dg.Rows.Count - dg.FrozenBottomRowsCount == 0)
            {
                // 불량정보가 없습니다.
                Util.MessageValidation("SFU1585");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dg, "CHK") < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 불량 대차 구성 완료 팝업
        /// </summary>
        private void DefectCartComplet()
        {
            CMM_POLYMER_FORM_CART_DEFECT_COMPLET popupCartDefectComplet = new CMM_POLYMER_FORM_CART_DEFECT_COMPLET();
            popupCartDefectComplet.FrameOperation = this.FrameOperation;

            DataRow[] dr;

            if (((System.Windows.FrameworkElement)ctbDefectCart.SelectedItem).Name.Equals("DefectCart"))
            {
                dr = DataTableConverter.Convert(dgDefect.ItemsSource).Select();
            }
            else
            {
                dr = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = 1");
            }

            object[] parameters = new object[5];
            parameters[0] = _procID;
            parameters[1] = _procName;
            parameters[2] = _eqptID;
            parameters[3] = txtEquipment.Text;
            parameters[4] = dr;

            C1WindowExtension.SetParameters(popupCartDefectComplet, parameters);

            popupCartDefectComplet.Closed += new EventHandler(popupCartDefectComplet_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCartDefectComplet);
                    popupCartDefectComplet.BringToFront();
                    break;
                }
            }
        }

        private void popupCartDefectComplet_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CART_DEFECT_COMPLET popup = sender as CMM_POLYMER_FORM_CART_DEFECT_COMPLET;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                // OK인 경우 스캔자료 Clear
                if (((System.Windows.FrameworkElement)ctbDefectCart.SelectedItem).Name.Equals("DefectCart"))
                {
                    Util.gridClear(dgDefect);
                }
            }

            // List에서 대차 구성시 재조회
            if (((System.Windows.FrameworkElement)ctbDefectCart.SelectedItem).Name.Equals("DefectList"))
            {
                DefectSearch();
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

        }

        /// <summary>
        /// Cell 등록 팝업
        /// </summary>
        private void CellInsert()
        {
            CMM_POLYMER_CELL_INPUT popupCellInsert = new CMM_POLYMER_CELL_INPUT();
            popupCellInsert.FrameOperation = this.FrameOperation;

            popupCellInsert.CTNR_DEFC_LOT_CHK = "N";

            object[] parameters = new object[1];
            parameters[0] = _defectLotID;

            C1WindowExtension.SetParameters(popupCellInsert, parameters);

            popupCellInsert.Closed += new EventHandler(popupCellInsert_Closed);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupCellInsert);
                    popupCellInsert.BringToFront();
                    break;
                }
            }
        }

        private void popupCellInsert_Closed(object sender, EventArgs e)
        {
            InitializeUserControls();

            CMM_POLYMER_CELL_INPUT popup = sender as CMM_POLYMER_CELL_INPUT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                DefectSearch();
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }
        }

        private void tbCheckHeaderAllDefect_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgDefect;
            if (dg?.ItemsSource == null) return;

            foreach (DataGridRow row in dg.Rows)
            {
                switch (_inBoxHeaderType)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_inBoxHeaderType)
            {
                case CheckBoxHeaderType.Zero:
                    _inBoxHeaderType = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _inBoxHeaderType = CheckBoxHeaderType.Zero;
                    break;
            }

        }

        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgList;
            if (dg?.ItemsSource == null) return;

            if (_defectList == null || _defectList.Rows.Count == 0) return;

            int chkrow = -1;
            bool rowCheck = true;

            DataRow[] dr = _defectList.Select("CHKYN = 'Y'");
            if (dr.Length == 0)
            {
                chkrow = 0;
            }
            else
            {
                chkrow = _defectList.Rows.IndexOf(dr[0]);
                _defectList.Rows[chkrow]["CHKYN"] = "N";

                if (chkrow + 1 == _defectList.Rows.Count)
                {
                    chkrow = 0;
                    if (_defectList.Rows.Count == 1)
                    {
                        rowCheck = false;
                    }
                }
                else
                {
                    chkrow++;
                }
            }

            if (rowCheck)
               _defectList.Rows[chkrow]["CHKYN"] = "Y";

            _defectList.AcceptChanges();

            foreach (DataGridRow row in dg.Rows)
            {
                if (Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRODID")) == Util.NVC(_defectList.Rows[chkrow]["PRODID"]) &&
                    Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRODID")) == Util.NVC(_defectList.Rows[chkrow]["PRODID"]))
                {
                    if (rowCheck)
                    {
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                    }
                    else
                    {
                        // Row가 한건인 경우 체크 해제
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                    }
                }
                else
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", false);
                }

            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void GridRowDelete()
        {
            DataTable dt = DataTableConverter.Convert(dgDefect.ItemsSource);

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(row => row.Delete());
            dt.AcceptChanges();

            Util.GridSetData(dgDefect, dt, FrameOperation);
        }

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
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