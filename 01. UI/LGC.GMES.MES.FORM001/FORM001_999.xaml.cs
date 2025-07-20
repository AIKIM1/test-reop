/*************************************************************************************
 Created Date : 2017.12.07
      Creator : 
   Decription : 파우치 수동 대차 Sheet 발행
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
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_999 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        private int _tagPrintCount;

        DataTable _dtResult;
        DataTable _dtProductBOM;

        public FORM001_999()
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
            // 대차 Sheet 발행
            SetAreaCombo(cboArea);
            SetProcessCombo(cboProcess);
            SetEquipmentSegmentCombo(cboEquipmentSegment);
            SetEquipmentCombo(cboEquipment, cboEquipmentSegment, cboProcess);
            SetCartStatCodeCombo();
            SetGridCapaGrade();

            // 작업구분
            //CommonCombo combo = new CommonCombo();
            //string[] sFilter = { LoginInfo.CFG_AREA_ID, LoginInfo.CFG_EQSG_ID };
            //combo.SetCombo(cboFormWorkType, CommonCombo.ComboStatus.SELECT, sCase: "FORM_WRK_TYPE_CODE_LINE", sFilter: sFilter);
            // 시장유형
            CommonCombo _combo = new CommonCombo();
            string[] sFilter = { "MKT_TYPE_CODE" };
            _combo.SetCombo(cboFormWorkType, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter);

            //cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
            cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;

            // 발행 이력 조회
            SetAreaCombo(cboAreaHistory);
            SetEquipmentSegmentHistoryCombo(cboEquipmentSegmentHistory);
            SetProcessHistoryCombo(cboProcessHistory);
            SetEquipmentCombo(cboEquipmentHistory, cboEquipmentSegmentHistory, cboProcessHistory);

            cboAreaHistory.SelectedValueChanged += cboAreaHistory_SelectedValueChanged;
            cboEquipmentSegmentHistory.SelectedValueChanged += cboEquipmentSegmentHistory_SelectedValueChanged;
            cboProcessHistory.SelectedValueChanged += cboProcessHistory_SelectedValueChanged;
        }

        #endregion

        #region Event

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnDelete);
            listAuth.Add(btnPrint);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            //rdoPKG.IsEnabled = false;

            txtAssyLot.Focus();

            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [대차 Sheet 발행]
        private void txtAssyLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!ValidationSelect()) return;

                    SetAssyLotSelect();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
            //    return;

            cboProcess.SelectedValueChanged -= cboProcess_SelectedValueChanged;

            SetEquipmentSegmentCombo(cboEquipmentSegment);
            SetProcessCombo(cboProcess);
            SetGridCapaGrade();

            //SetFormWorkTypeCombo(cboFormWorkType, cboArea, cboEquipmentSegment);

            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString().Equals("SELECT"))
            //    return;

            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString().Equals("SELECT") 
                || (cboProcess.SelectedValue.ToString() != "B1000" && cboProcess.SelectedValue.ToString() != "B9000"))
            {
                rdoPKG.IsEnabled = false;
                rdoDSF.IsEnabled = false;
                rdoTaping.IsEnabled = false;
                rdoTCO.IsEnabled = false;

                rdoPKG.IsChecked = false;
                rdoDSF.IsChecked = false;
                rdoTaping.IsChecked = false;
                rdoTCO.IsChecked = false;
            }
            else
            {
                rdoPKG.IsEnabled = true;
                rdoDSF.IsEnabled = true;
                rdoTaping.IsEnabled = true;
                rdoTCO.IsEnabled = true;
            }

            SetEquipmentSegmentCombo(cboEquipmentSegment);
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (cboEquipmentSegment.SelectedValue == null || cboEquipmentSegment.SelectedValue.ToString().Equals("SELECT"))
            //    return;

            SetEquipmentCombo(cboEquipment, cboEquipmentSegment, cboProcess);
            SetGridCapaGrade();

            //SetFormWorkTypeCombo(cboFormWorkType, cboArea, cboEquipmentSegment);
        }

        private void dgAssyList_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1ComboBox cbo = e.EditingElement as C1ComboBox;

            if (cbo != null)
            {
                if (e.Column.Name == "cboFCSProdID")
                {
                    DataTable dt = GetProduct(Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "PROCID")),
                                              Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "CLASS_CODE")),
                                              Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "CARTSTATCODE")),
                                              Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "PRODID")));

                    if (dt != null && dt.Rows.Count > 1)
                    {
                        DataRow dr = dt.NewRow();
                        dr["FCSPRODID"] = "SELECT";
                        dr["FCSPRODNAME"] = "- SELECT -";
                        dt.Rows.InsertAt(dr, 0);
                    }
                    dt.AcceptChanges();

                    cbo.ItemsSource = DataTableConverter.Convert(dt.Copy());
                    cbo.SelectedIndex = 0;
                }
            }

        }


        private void dgAssyList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (!e.Cell.Row.Type.Equals(DataGridRowType.Item))
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

                if (e.Cell.Column.Name.Equals("cboFCSProdID") || e.Cell.Column.Name.Equals("CELL_QTY"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                }

                if (!e.Cell.Column.Name.Equals("CHK"))
                {
                    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(150);
                }

            }));

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // 변경전 자료 반영
            dgAssyList.EndEditRow(true);

            if (!ValidationDelete(dgAssyList))
                return;

            // 저장안된 row 삭제
            GridRowDelete();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave())
                return;

            // 저장하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    AssyLotSave(false);
                }
            });

        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave())
                return;

            // 발행하시겠습니까?
            Util.MessageConfirm("SFU2873", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    AssyLotSave(true);
                }
            });

        }

        #endregion

        #region [발행 이력 조회]
        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationHistory())
                return;

            GetHistory();
        }

        private void btnDeleteHistory_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDelete(dgHistory))
                return;

            // 대차 Sheet 발행 내역을 삭제 하시겠습니까?
            Util.MessageConfirm("SFU4317", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    AssyLotDelete();
                }
            });
        }

        private void btnPrintHistory_Click(object sender, RoutedEventArgs e)
        {
            SetSheetRePrint();
        }

        private void cboAreaHistory_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (cboAreaHistory.SelectedValue == null || cboAreaHistory.SelectedValue.ToString().Equals("SELECT"))
            //    return;

            cboEquipmentSegmentHistory.SelectedValueChanged -= cboEquipmentSegmentHistory_SelectedValueChanged;

            SetEquipmentSegmentHistoryCombo(cboEquipmentSegmentHistory);
            SetProcessHistoryCombo(cboProcessHistory);

            cboEquipmentSegmentHistory.SelectedValueChanged += cboEquipmentSegmentHistory_SelectedValueChanged;
        }

        private void cboEquipmentSegmentHistory_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (cboEquipmentSegmentHistory.SelectedValue == null || cboEquipmentSegmentHistory.SelectedValue.ToString().Equals("SELECT"))
            //    return;

            SetProcessHistoryCombo(cboProcessHistory);
        }

        private void cboProcessHistory_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (cboProcessHistory.SelectedValue == null || cboProcessHistory.SelectedValue.ToString().Equals("SELECT"))
            //    return;

            SetEquipmentCombo(cboEquipmentHistory, cboEquipmentSegmentHistory, cboProcessHistory);
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                //if ((dtpDateToOut.SelectedDateTime - dtpDateFromOut.SelectedDateTime).TotalDays > 7)
                //{
                //    //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                //    Util.MessageValidation("SFU2042", "7");

                //    dtpDateFromOut.SelectedDateTime = dtpDateToOut.SelectedDateTime.AddDays(-6);
                //    return;
                //}

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
        }

        #endregion

        #endregion

        #region Mehod

        #region [BizCall]

        #region 용량 등급
        private void SetGridCapaGrade()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("QLTY_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                //newRow["QLTY_TYPE_CODE"] = "G";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_GRADE_TYPE_CODE_LINE_CBO", "INDATA", "OUTDATA", inTable);

                ////DataRow dr = dtResult.NewRow();
                ////dr["CBO_CODE"] = "";
                ////dr["CBO_NAME"] = " - SELECT-";
                ////dtResult.Rows.InsertAt(dr, 0);

                (dgAssyList.Columns["CAPA_GRD_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult.Copy());

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region 대차 Sheet 발행
        /// <summary>
        /// Assy List 조회
        /// </summary>
        /// <param name="bButton"></param>
        private void GridRowADD()
        {
            try
            {
                ShowLoadingIndicator();
                //DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LOTID"] = txtAssyLot.Text;

                dtRqst.Rows.Add(dr);

                _dtResult = new DataTable();
                _dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_N12345_LOT_INFO", "INDATA", "OUTDATA", dtRqst);

                if (_dtResult == null || _dtResult.Rows.Count == 0)
                {
                    HiddenLoadingIndicator();

                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    return;
                }

                ////dtRslt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
                DataTable dtSource = DataTableConverter.Convert(dgAssyList.ItemsSource);
                SetProductCombo();

                if (dtSource == null || dtSource.Rows.Count == 0)
                {
                    dtSource = _dtResult.Copy();
                }
                else
                {
                    //DataRow[] drSelect = dtSource.Select("ASSY_LOTID = '" + txtAssyLot.Text.Substring(0,8) + "'");
                    //if (drSelect.Length == 0)
                    //{
                    //    dtSource.Merge(_dtResult);
                    //}

                    DataRow drSelect = dtSource.NewRow();
                    drSelect["CHK"] = _dtResult.Rows[0]["CHK"];
                    drSelect["ASSY_LOTID"] = _dtResult.Rows[0]["ASSY_LOTID"];
                    drSelect["PRJT_NAME"] = _dtResult.Rows[0]["PRJT_NAME"];
                    drSelect["PRODID"] = _dtResult.Rows[0]["PRODID"];
                    drSelect["INBOX_QTY"] = _dtResult.Rows[0]["INBOX_QTY"];
                    drSelect["CELL_QTY"] = _dtResult.Rows[0]["CELL_QTY"];
                    drSelect["PROCID"] = _dtResult.Rows[0]["PROCID"];
                    drSelect["CARTSTATCODE"] = _dtResult.Rows[0]["CARTSTATCODE"];
                    drSelect["CLASS_CODE"] = _dtResult.Rows[0]["CLASS_CODE"];
                    drSelect["FCSPRODID"] = _dtResult.Rows[0]["FCSPRODID"];
                    drSelect["PRODID_KEYIN"] = _dtResult.Rows[0]["PRODID_KEYIN"];
                    drSelect["CAPA_GRD_CODE"] = _dtResult.Rows[0]["CAPA_GRD_CODE"];

                    dtSource.Rows.Add(drSelect);

                }

                if (_dtProductBOM != null && _dtProductBOM.Rows.Count > 0)
                {
                    (dgAssyList.Columns["cboFCSProdID"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(_dtProductBOM.Copy());
                }

                Util.GridSetData(dgAssyList, dtSource, FrameOperation, true);

                //txtAssyLot.Text = string.Empty;
                txtAssyLot.Focus();

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private DataTable GetProduct(string ProcID, string ClassCode, string CartStatCode, string mtrlID)
        {
            try
            {
                string ClassCodeList = SetClassCode(ProcID, ClassCode, CartStatCode);

                DataTable dt = new DataTable();
                if (string.IsNullOrWhiteSpace(ClassCodeList))
                {
                    dt.Columns.Add("FCSPRODID", typeof(string));
                    dt.Columns.Add("FCSPRODNAME", typeof(string));

                    DataRow dr = dt.NewRow();
                    dr["FCSPRODID"] = mtrlID;
                    dt.Rows.Add(dr);
                }
                else
                {
                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("MTRLID", typeof(string));
                    inTable.Columns.Add("CLASS_CODE", typeof(string));

                    DataRow newRow = inTable.NewRow();
                    newRow["MTRLID"] = mtrlID;
                    newRow["CLASS_CODE"] = ClassCodeList;
                    inTable.Rows.Add(newRow);
                    dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_BOM_CBO_PC", "INDATA", "OUTDATA", inTable);
                }

                return dt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void SetProductCombo()
        {
            try
            {
                // Class Code 정보 
                string ClassCode = string.Empty;
                if ((bool)rdoPKG.IsChecked)
                    ClassCode = "PKG";
                else if ((bool)rdoDSF.IsChecked)
                    ClassCode = "DSF";
                else if ((bool)rdoTaping.IsChecked)
                    ClassCode = "TAPING";
                else if ((bool)rdoTCO.IsChecked)
                    ClassCode = "TCO";
                else
                    ClassCode = "";

                // BOM 검색용 칼럼 추가
                if (!_dtResult.Columns.Contains("PROCID"))
                    _dtResult.Columns.Add("PROCID", typeof(string));
                if (!_dtResult.Columns.Contains("CARTSTATCODE"))
                    _dtResult.Columns.Add("CARTSTATCODE", typeof(string));
                if (!_dtResult.Columns.Contains("CLASS_CODE"))
                    _dtResult.Columns.Add("CLASS_CODE", typeof(string));
                if (!_dtResult.Columns.Contains("FCSPRODID"))
                    _dtResult.Columns.Add("FCSPRODID", typeof(string));
                if (!_dtResult.Columns.Contains("PRODID_KEYIN"))
                    _dtResult.Columns.Add("PRODID_KEYIN", typeof(string));
                if (!_dtResult.Columns.Contains("CAPA_GRD_CODE"))
                    _dtResult.Columns.Add("CAPA_GRD_CODE", typeof(string));

                _dtResult.Rows[0]["PROCID"] = cboProcess.SelectedValue.ToString();
                _dtResult.Rows[0]["CARTSTATCODE"] = cboCartStatCode.SelectedValue.ToString();
                _dtResult.Rows[0]["CLASS_CODE"] = ClassCode;

                // BOM 산출
                DataTable dt = GetProduct(cboProcess.SelectedValue.ToString(), ClassCode, cboCartStatCode.SelectedValue.ToString(), _dtResult.Rows[0]["PRODID"].ToString());

                if (_dtProductBOM == null || _dtProductBOM.Rows.Count == 0)
                {
                    _dtProductBOM = dt.Copy();
                }
                else
                {
                    //DataRow[] drSelect = _dtProductBOM.Select("FCSPRODID = '" + _dtResult.Rows[0]["PRODID"].ToString() + "'");
                    
                    foreach (DataRow drSelect in dt.Rows)
                    {
                        DataRow[] drAdd = _dtProductBOM.Select("FCSPRODID = '" + drSelect["FCSPRODID"].ToString() + "'");

                        if (drAdd.Length == 0)
                        {
                            _dtProductBOM.Merge(dt);
                        }
                    }
                }

                if (dt == null || dt.Rows.Count == 0)
                {
                    // 없다면???
                }
                else
                {
                    if (_dtResult.Rows[0]["PRODID"].ToString().Equals(dt.Rows[0]["FCSPRODID"].ToString()))
                    {
                        _dtResult.Rows[0]["FCSPRODID"] = dt.Rows[0]["FCSPRODID"].ToString();
                    }
                    else
                    {
                        if (dt.Rows.Count == 1)
                            _dtResult.Rows[0]["FCSPRODID"] = dt.Rows[0]["FCSPRODID"].ToString();
                        else
                            _dtResult.Rows[0]["FCSPRODID"] = "SELECT";
                    }

                    DataRow[] drSelect = _dtProductBOM.Select("FCSPRODID = 'SELECT'");

                    if (drSelect.Length == 0)
                    {
                        DataRow dr = _dtProductBOM.NewRow();
                        dr["FCSPRODID"] = "SELECT";
                        dr["FCSPRODNAME"] = "- SELECT -";
                        _dtProductBOM.Rows.InsertAt(dr, 0);
                    }

                }

                _dtProductBOM.AcceptChanges();
                _dtResult.AcceptChanges();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void AssyLotSave(bool Print = false)
        {
            try
            {
                ShowLoadingIndicator();

                // DATA SET
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CART_STAT_CODE", typeof(string));
                inTable.Columns.Add("CART_GNRT_DTTM", typeof(string));

                DataTable inAssyLot = inDataSet.Tables.Add("IN_ASSYLOT");
                inAssyLot.Columns.Add("CART_ID", typeof(string));
                inAssyLot.Columns.Add("ASSY_LOTID", typeof(string));
                inAssyLot.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inAssyLot.Columns.Add("PRODID", typeof(string));
                inAssyLot.Columns.Add("INBOX_QTY", typeof(Decimal));
                inAssyLot.Columns.Add("CELL_QTY", typeof(Decimal));

                // INDATA SET  Util.GetCondition(dtpDateFrom);
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = "INS";
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROCID"] = cboProcess.SelectedValue.ToString();
                newRow["FORM_WRK_TYPE_CODE"] = cboFormWorkType.SelectedValue.ToString();
                newRow["USE_FLAG"] = "Y";
                newRow["USERID"] = LoginInfo.USERID;
                newRow["CART_STAT_CODE"] = cboCartStatCode.SelectedValue.ToString();
                newRow["CART_GNRT_DTTM"] = dtpDate.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                inTable.Rows.Add(newRow);

                newRow = null;
                foreach (DataGridRow dRow in dgAssyList.Rows)
                {
                    if (dRow.Type.Equals(DataGridRowType.Top) || dRow.Type.Equals(DataGridRowType.Bottom))
                        continue;

                    newRow = inAssyLot.NewRow();
                    newRow["ASSY_LOTID"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "ASSY_LOTID"));
                    newRow["CAPA_GRD_CODE"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "CAPA_GRD_CODE"));
                    if (Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "PRODID_KEYIN")).Equals(""))
                    {
                        newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "FCSPRODID"));
                    }
                    else
                    {
                        newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "PRODID_KEYIN"));
                    }
                    newRow["INBOX_QTY"] = Util.NVC_Int(DataTableConverter.GetValue(dRow.DataItem, "INBOX_QTY"));
                    newRow["CELL_QTY"] = Util.NVC_Int(DataTableConverter.GetValue(dRow.DataItem, "CELL_QTY"));
                    inAssyLot.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_TB_SFC_CART_SHEET", "INDATA,IN_ASSYLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        if (bizResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            if (Print)
                            {
                                SetSheetPrint(bizResult.Tables["OUTDATA"].Rows[0]["CART_ID"].ToString());
                            }
                            else
                            {
                                Util.gridClear(dgAssyList);
                                txtAssyLot.Focus();
                            }
                        }

                        // Clear
                        _dtProductBOM = new DataTable();

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
        private void GetHistory()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FRDATE", typeof(string));
                dtRqst.Columns.Add("TODATE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("ASSY_LOTID", typeof(string));
                dtRqst.Columns.Add("CART_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                if (string.IsNullOrWhiteSpace(txtPrjtNameHistory.Text) &&
                    string.IsNullOrWhiteSpace(txtProdidHistory.Text) &&
                    string.IsNullOrWhiteSpace(txtAssyLotHistory.Text) &&
                    string.IsNullOrWhiteSpace(txtCartIDHistory.Text))
                {
                    dr["FRDATE"] = Util.GetCondition(dtpDateFrom);
                    dr["TODATE"] = Util.GetCondition(dtpDateTo);
                    dr["AREAID"] = cboAreaHistory.SelectedValue.ToString();
                    if (cboProcessHistory.SelectedValue == null || cboProcessHistory.SelectedValue.ToString().Equals("SELECT"))
                        dr["PROCID"] = null;
                    else
                        dr["PROCID"] = cboProcessHistory.SelectedValue.ToString();

                    if (cboEquipmentHistory.SelectedValue == null || cboEquipmentHistory.SelectedValue.ToString().Equals("SELECT"))
                        dr["EQPTID"] = null;
                    else
                        dr["EQPTID"] = cboEquipmentHistory.SelectedValue.ToString();
                }
                else
                {
                    dr["PRJT_NAME"] = txtPrjtNameHistory.Text;
                    dr["PRODID"] = txtProdidHistory.Text;
                    dr["ASSY_LOTID"] = txtAssyLotHistory.Text;
                    dr["CART_ID"] = txtCartIDHistory.Text;
                }
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CART_SHEET_LIST", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgHistory, dtRslt, FrameOperation, true);

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void AssyLotDelete()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA SET
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inAssyLot = inDataSet.Tables.Add("IN_ASSYLOT");
                inAssyLot.Columns.Add("CART_ID", typeof(string));
                inAssyLot.Columns.Add("ASSY_LOTID", typeof(string));
                inAssyLot.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inAssyLot.Columns.Add("PRODID", typeof(string));
                inAssyLot.Columns.Add("INBOX_QTY", typeof(Decimal));
                inAssyLot.Columns.Add("CELL_QTY", typeof(Decimal));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = "DEL";
                newRow["USE_FLAG"] = "N";
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable dt = DataTableConverter.Convert(dgHistory.ItemsSource);
                DataRow[] dr = dt.Select("CHK = 1");

                newRow = null;
                foreach (DataRow dRow in dr)
                {
                    newRow = inAssyLot.NewRow();
                    newRow["CART_ID"] = Util.NVC(dRow["CART_ID"]);
                    newRow["ASSY_LOTID"] = Util.NVC(dRow["ASSY_LOTID"]);
                    newRow["CAPA_GRD_CODE"] = Util.NVC(dRow["CAPA_GRD_CODE"]);
                    inAssyLot.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_TB_SFC_CART_SHEET", "INDATA,IN_ASSYLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        dt.Select("CHK = 1").ToList<DataRow>().ForEach(row => row.Delete());
                        dt.AcceptChanges();

                        Util.GridSetData(dgHistory, dt, FrameOperation);
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

        #endregion

        #region [Validation]

        private bool ValidationSelect()
        {
            if (string.IsNullOrWhiteSpace(txtAssyLot.Text))
            {
                // 조립LOTID는 8자리 이상입니다.
                Util.MessageValidation("SFU4075");
                return false;
            }

            if (txtAssyLot.Text.Trim().Length < 8)
            {
                // 조립LOTID는 8자리 이상입니다.
                Util.MessageValidation("SFU4075");
                return false;
            }

            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.GetString().Equals("SELECT"))
            {
                // 공정을 선택하세요.
                Util.MessageValidation("SFU1459");
                return false;
            }

            if (cboCartStatCode.SelectedValue == null || cboCartStatCode.SelectedValue.GetString().Equals("SELECT"))
            {
                // 대차상태 정보를 선택하세요.
                Util.MessageValidation("SFU4372");
                return false;
            }

            if (cboProcess.SelectedValue.ToString().Equals("B1000") || cboProcess.SelectedValue.ToString().Equals("B9000"))
            {
                bool IsSelect = false;

                if ((bool)rdoPKG.IsChecked)
                    IsSelect = true;
                if ((bool)rdoDSF.IsChecked)
                    IsSelect = true;
                if ((bool)rdoTaping.IsChecked)
                    IsSelect = true;
                if ((bool)rdoTCO.IsChecked)
                    IsSelect = true;

                if (IsSelect == false)
                {
                    // 구분을 선택하세요
                    Util.MessageValidation("SFU4149");
                    return false;
                }
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

        private bool ValidationSave()
        {
            DataTable dt = DataTableConverter.Convert(dgAssyList.ItemsSource);

            if (dt == null || dt.Rows.Count == 0)
            {
                // 선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (dgAssyList.Rows.Count == 0)
            {
                // 발행정보가 없습니다.
                Util.MessageValidation("SFU3399");
                return false;
            }

            if (cboArea.SelectedValue == null || cboArea.SelectedValue.GetString().Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return false;
            }

            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.GetString().Equals("SELECT"))
            {
                // 공정을 선택하세요.
                Util.MessageValidation("SFU1459");
                return false;
            }

            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                // 설비를 선택하세요.
                Util.MessageValidation("SFU1153");
                return false;
            }

            if (cboFormWorkType.SelectedValue == null || cboFormWorkType.SelectedValue.GetString().Equals("SELECT"))
            {
                // 시장유형을 선택하세요.
                Util.MessageValidation("SFU4371");
                return false;
            }

            if (cboCartStatCode.SelectedValue == null || cboCartStatCode.SelectedValue.GetString().Equals("SELECT"))
            {
                // 대차상태 정보를 선택하세요.
                Util.MessageValidation("SFU4372");
                return false;
            }
   
            foreach (DataRow dRow in dt.Rows)
            {
                if (Util.NVC(dRow["ASSY_LOTID"]).Equals("") || Util.NVC(dRow["ASSY_LOTID"]).Length < 8)
                {
                    // 조립LOTID는 8자리 이상입니다.
                    Util.MessageValidation("SFU4075");
                    return false;
                }

                if (Util.NVC(dRow["FCSPRODID"]).Equals("") || Util.NVC(dRow["FCSPRODID"]).Equals("SELECT"))
                {
                    if (Util.NVC(dRow["PRODID_KEYIN"]).Equals(""))
                    {
                        // 활성화 제품 코드가 없습니다.
                        Util.MessageValidation("SFU4405");
                        return false;
                    }
                }

                if (Util.NVC(dRow["CAPA_GRD_CODE"]).Equals(""))
                {
                    // 용량등급을 선택해 주세요.
                    Util.MessageValidation("SFU4022");
                    return false;
                }

                if (Util.NVC_Int(dRow["CELL_QTY"]) == 0)
                {
                    // 수량은 0보다 큰 정수로 입력 하세요.
                    Util.MessageValidation("SFU3092");
                    return false;
                }

                // 중복 체크
                DataRow[] drDup = dt.Select("ASSY_LOTID = '" + Util.NVC(dRow["ASSY_LOTID"]) + "' And CAPA_GRD_CODE = '" 
                                                             + Util.NVC(dRow["CAPA_GRD_CODE"]) + "'");

                if (drDup.Length > 1)
                {
                    for (int nrow = 0; nrow < drDup.Length; nrow++)
                    {
                        int rowindex = dt.Rows.IndexOf(drDup[nrow]);

                        dgAssyList.GetCell(rowindex, 1).Presenter.Background = new SolidColorBrush(Colors.Red);
                    }

                    // 중복된 조립LOT 정보가 존재합니다.
                    Util.MessageValidation("SFU4226");
                    return false;
                }
            }

            return true;
        }

        private bool ValidationHistory()
        {
            if (cboAreaHistory.SelectedValue == null || cboAreaHistory.SelectedValue.GetString().Equals("SELECT"))
            {
                // 동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return false;
            }

            ////if (cboProcessHistory.SelectedValue == null || cboProcessHistory.SelectedValue.GetString().Equals("SELECT"))
            ////{
            ////    // 공정을 선택하세요.
            ////    Util.MessageValidation("SFU1459");
            ////    return false;
            ////}

            ////if (cboEquipmentHistory.SelectedValue == null || cboEquipmentHistory.SelectedValue.GetString().Equals("SELECT"))
            ////{
            ////    // 설비를 선택하세요.
            ////    Util.MessageValidation("SFU1153");
            ////    return false;
            ////}

            return true;
        }

        private bool ValidationPrint()
        {
            if (dgAssyList.Rows.Count == 0)
            {
                // 선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return false;
            }

            return true;
        }

        #endregion

        #region [Func]
        private void SetAreaCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_AREA_CBO";
            string[] arrColumn = { "LANGID", "SHOPID"};
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);
        }

        private void SetProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_PROCESS_AREA_CBO_PC";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, Util.NVC(cboArea.SelectedValue) };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_PROC_ID);
        }

        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_PC";
            string[] arrColumn = { "LANGID", "AREAID", "PROCID" };
            string[] arrCondition = { LoginInfo.LANGID, Util.NVC(cboArea.SelectedValue), Util.NVC(cboProcess.SelectedValue) };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;
            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }

        private void SetEquipmentSegmentHistoryCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            string[] arrColumn = { "LANGID", "AREAID", };
            string[] arrCondition = { LoginInfo.LANGID, Util.NVC(cboAreaHistory.SelectedValue) };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }

        private void SetProcessHistoryCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";
            string[] arrColumn = { "LANGID", "EQSGID" };
            string[] arrCondition = { LoginInfo.LANGID, Util.NVC(cboEquipmentSegmentHistory.SelectedValue) };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_PROC_ID);
        }

        private void SetEquipmentCombo(C1ComboBox cbo, C1ComboBox cboParent, C1ComboBox cboParent2)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_CBO";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "COATER_EQPT_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, Util.NVC(cboParent.SelectedValue), Util.NVC(cboParent2.SelectedValue), null };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private void SetFormWorkTypeCombo(C1ComboBox cbo, C1ComboBox cboParent, C1ComboBox cboParent2)
        {
            const string bizRuleName = "DA_PRD_SEL_FORM_WRK_TYPE_CODE_LINE_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID" };
            string[] arrCondition = { LoginInfo.LANGID, Util.NVC(cboParent.SelectedValue), Util.NVC(cboParent2.SelectedValue), null };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, null);
        }

        private void SetCartStatCodeCombo()
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add(cboCartStatCode.SelectedValuePath.ToString(), typeof(string));
            dtResult.Columns.Add(cboCartStatCode.DisplayMemberPath.ToString(), typeof(string));

            DataRow dr = dtResult.NewRow();
            dr[cboCartStatCode.DisplayMemberPath.ToString()] = "-SELECT-";
            dr[cboCartStatCode.SelectedValuePath.ToString()] = "SELECT";
            dtResult.Rows.InsertAt(dr, 0);

            dr = dtResult.NewRow();
            dr[cboCartStatCode.DisplayMemberPath.ToString()] = ObjectDic.Instance.GetObjectName(Util.NVC("대기"));
            dr[cboCartStatCode.SelectedValuePath.ToString()] = "WAIT";
            dtResult.Rows.InsertAt(dr, 1);

            dr = dtResult.NewRow();
            dr[cboCartStatCode.DisplayMemberPath.ToString()] = ObjectDic.Instance.GetObjectName(Util.NVC("완료"));
            dr[cboCartStatCode.SelectedValuePath.ToString()] = "END";
            dtResult.Rows.InsertAt(dr, 2);

            cboCartStatCode.DisplayMemberPath = cboCartStatCode.DisplayMemberPath.ToString();
            cboCartStatCode.SelectedValuePath = cboCartStatCode.SelectedValuePath.ToString();
            cboCartStatCode.ItemsSource = dtResult.Copy().AsDataView();

            cboCartStatCode.SelectedIndex = 0;
        }

        private void SetAssyLotSelect()
        {
            //bool isList = false;

            //if (dgAssyList.ItemsSource != null)
            //{
            //    DataTable dt = DataTableConverter.Convert(dgAssyList.ItemsSource);
            //    DataRow[] dr = dt.Select("ASSY_LOTID = '" + txtAssyLot.Text.Substring(0,8) + "'");

            //    if (dr.Length > 0)
            //    {
            //        // 조회 목록에 있다
            //        isList = true;
            //        int idx = dt.Rows.IndexOf(dr[0]);

            //        dt.Rows[idx]["CHK"] = false;
            //        DataTableConverter.SetValue(dgAssyList.Rows[idx].DataItem, "CHK", false);
            //        dgAssyList.SelectedIndex = idx;
            //        dgAssyList.ScrollIntoView(idx, dgAssyList.Columns["CHK"].Index);

            //        txtAssyLot.Focus();
            //        txtAssyLot.Text = string.Empty;
            //    }
            //}

            //if ((bool)isList == false)
            //{
                // 조회후 Grid에 ADD
                GridRowADD();
            //}

        }

        private void GridRowDelete()
        {
            DataTable dt = DataTableConverter.Convert(dgAssyList.ItemsSource);

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(row => row.Delete());
            dt.AcceptChanges();

            Util.GridSetData(dgAssyList, dt, FrameOperation);
        }

        // Sheet 발행
        private void SetSheetPrint(string cartID)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupTagPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;

            popupTagPrint.PrintCount = "1";


            object[] parameters = new object[5];
            parameters[0] = "";       // _processCode;
            parameters[1] = "";       // Util.NVC(ComboEquipment.SelectedValue);
            parameters[2] = cartID;
            parameters[3] = "N";      // Direct 출력 여부
            parameters[4] = "Y";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupTagPrint, parameters);

            popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);
            grdMain.Children.Add(popupTagPrint);
            popupTagPrint.BringToFront();
        }

        // Sheet 재발행
        private void SetSheetRePrint()
        {
            if (dgHistory.Rows.Count == 0)
            {
                // 선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgHistory.ItemsSource).Select("CHK = 1").CopyToDataTable().DefaultView.ToTable(true, "CART_ID");

            if (dt.Rows.Count == 0)
            {
                // 선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return;
            }

            _tagPrintCount = dt.Rows.Count;

            foreach (DataRow drPrint in dt.Rows)
            {
                TagPrint(drPrint);
            }

        }

        private void TagPrint(DataRow dr)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupTagPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;

            popupTagPrint.PrintCount = _tagPrintCount.ToString();

            _tagPrintCount--;

            object[] parameters = new object[5];
            parameters[0] = "";       // _processCode;
            parameters[1] = "";       // Util.NVC(ComboEquipment.SelectedValue);
            parameters[2] = dr["CART_ID"];
            parameters[3] = "N";      // Direct 출력 여부
            parameters[4] = "Y";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupTagPrint, parameters);

            popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);
            grdMain.Children.Add(popupTagPrint);
            popupTagPrint.BringToFront();
        }

        private void popupTagPrint_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_TAG_PRINT popup = sender as CMM_POLYMER_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(popup);

            Util.gridClear(dgAssyList);
            txtAssyLot.Focus();

        }

        private string SetClassCode(string ProcID, string ClassCode, string CartStatCode)
        {
            string ReturnList = string.Empty;

            switch (ProcID)
            {
                // Grading, 특성Grading, offline 특성측정, 특성 최종외관검사
                case "F5400":
                case "F5500":
                case "F5600":
                case "F8100":
                    ReturnList = null;
                    break;
                // DSF
                case "F7000":
                    if (CartStatCode.Equals("END"))
                        ReturnList = "MDP,MDF";
                    else
                        ReturnList = null;

                    break;

                // TCO
                case "F7100":
                    if (CartStatCode.Equals("END"))
                        ReturnList = "MTR,MTP,MTF";
                    else
                        ReturnList = null;

                    break;

                // Taping, Side Taping
                case "F7200":
                case "F7300":
                    if (CartStatCode.Equals("END"))
                        ReturnList = "MIP,MIF";
                    else
                        ReturnList = null;

                    break;

                // DSF 양품화, DSF 최종외관검사
                case "F7500":
                case "F8000":
                    ReturnList = "MDP,MDF";
                    break;

                // Side Taping 양품화
                case "F7600":
                    ReturnList = "MIP,MIF";
                    break;

                // Cell 포장, 물류반품
                case "B1000":
                case "B9000":
                    if (ClassCode.Equals("DSF"))
                        ReturnList = "MDP,MDF";
                    else if (ClassCode.Equals("TAPING"))
                        ReturnList = "MIP,MIF";
                    else if (ClassCode.Equals("TCO"))
                        ReturnList = "MTR,MTP,MTF";
                    else
                        ReturnList = null;
                    break;

            }

            return ReturnList;
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
