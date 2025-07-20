/*************************************************************************************
 Created Date : 2019.05.23
      Creator : INS 김동일K
   Decription : CWA3동 증설 - LOT MERGE 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.23  INS 김동일K : Initial Created.
  2019.06.12  정문교      : 비정상수량여부 추가 및 수량 수정 가능하게 수정
  2019.06.28  정문교      : PDA와 동일하게 수정
  2019.07.10  정문교      : 메사지 변경
  2019.10.16  정문교      : BR_PRD_REG_MERGE_INOUT_LOT_ASSY -> BR_PRD_REG_MERGE_INOUT_LOT_ASSY_L로 변경
  2019.11.13  정문교      : 비정상 수량 수정시 최대 수량 체크 로직 추가

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
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_OUTLOT_MERGE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_OUTLOT_MERGE : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LineName = string.Empty;
        private string _Procid = string.Empty;

        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _util = new Util();

        double _max_LOAD_QTY;
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY004_OUTLOT_MERGE()
        {
            InitializeComponent();
        }

        #endregion

        #region Event       
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 5)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _Procid = Util.NVC(tmps[2]);
                _LineName = Util.NVC(tmps[3]);

                _LDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[4]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _Procid = "";
                _LineName = "";

                _LDR_LOT_IDENT_BAS_CODE = "";
            }
            txtLotID.Focus();
            ApplyPermissions();

            if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID") || _LDR_LOT_IDENT_BAS_CODE.Equals("CST_ID"))
            {
                if (dgMerge.Columns.Contains("CSTID"))
                    dgMerge.Columns["CSTID"].Visibility = Visibility.Visible;

                tbLotID.Text = ObjectDic.Instance.GetObjectName("Carrier ID");
            }
            else
            {
                if (dgMerge.Columns.Contains("CSTID"))
                    dgMerge.Columns["CSTID"].Visibility = Visibility.Collapsed;

                tbLotID.Text = ObjectDic.Instance.GetObjectName("LOTID");
            }
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
                DataTable dtTmp = new DataTable();
                dtTmp.Columns.Add("CHK", typeof(Boolean));
                dtTmp.Columns.Add("LOTID", typeof(string));
                dtTmp.Columns.Add("CSTID", typeof(string));
                dtTmp.Columns.Add("WIPQTY", typeof(string));
                dtTmp.Columns.Add("PRODID", typeof(string));
                dtTmp.Columns.Add("PRODNAME", typeof(string));
                dtTmp.Columns.Add("PROCID", typeof(string));
                dtTmp.Columns.Add("PROCNAME", typeof(string));
                dtTmp.Columns.Add("WIPSTAT", typeof(string));
                dtTmp.Columns.Add("WIPSNAME", typeof(string));
                dtTmp.Columns.Add("EQSGID", typeof(string));
                dtTmp.Columns.Add("EQSGNAME", typeof(string));
                dtTmp.Columns.Add("ABNORM_QTY_FLAG", typeof(string));
                dtTmp.Columns.Add("MGZN_RECONF_FLAG", typeof(string));

                dgMerge.ItemsSource = DataTableConverter.Convert(dtTmp);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitializeControls()
        {
            _max_LOAD_QTY = 0;

            this.txtLotID.Text = string.Empty;
            this.txtLotID.Focus();
            this.txtTotQty.Text = string.Empty;
            this.txtScanCount.Text = string.Empty;
            this.txtPrjtName.Text = string.Empty;
            this.txtPrdtClssCode.Text = string.Empty;

            Util.gridClear(dgMerge);
        }


        /// <summary>
        /// 초기화
        /// </summary>
        private void btnInitialize_Click(object sender, RoutedEventArgs e)
        {
            InitializeControls();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgMerge.CommittedEdit -= dgMerge_CommittedEdit;
                dgMerge.EndEditRow(true);
                SumWipQty();
                dgMerge.CommittedEdit += dgMerge_CommittedEdit;

                if (!CanSave()) return;

                // 병합하시겠습니다?
                Util.MessageConfirm("SFU5111", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        MergeProcess();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLotID_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetLotInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgMergeChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && ((rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0") || (rb.DataContext as DataRowView).Row["CHK"].ToString().ToUpper().Equals("FALSE")))
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
                    dgMerge.SelectedIndex = idx;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 비정상 수량 체크가 Y인 경우 수량 수정 가능
        /// </summary>
        private void dgMerge_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name.Equals("WIPQTY"))
            {
                if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "ABNORM_QTY_FLAG")).Equals("N"))
                {
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// 수량 변경시 수량 합산
        /// </summary>
        private void dgMerge_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (e == null || e.Cell == null)
                return;

            SumWipQty();

            int rowIndex = e.Cell.Row.Index;

            // 비정상 수량 체크
            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ABNORM_QTY_FLAG")).Equals("Y"))
            {
                // Merge Max 수량 체크
                if (ScanMaxQtyCheck(0))
                {
                    //  최대 수량을 초과할 수 없습니다.
                    Util.MessageValidation("SFU5110", result =>
                    {
                        DataTableConverter.SetValue(e.Cell.Row.DataItem, "WIPQTY", 0);
                        // 수량 합산
                        SumWipQty();

                        //row 색 바꾸기
                        dgMerge.Focus();
                        dgMerge.SelectedIndex = rowIndex;
                        dgMerge.CurrentCell = dgMerge.GetCell(rowIndex, dgMerge.Columns["WIPQTY"].Index);
                    });
                }

                if (Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPQTY")) > 800)
                {
                    //  비정상 수량 수정시 최대 [%1]개를 초과할 수 없습니다.
                    Util.MessageValidation("SFU8125", result =>
                    {
                        DataTableConverter.SetValue(e.Cell.Row.DataItem, "WIPQTY", 0);
                        // 수량 합산
                        SumWipQty();

                        //row 색 바꾸기
                        dgMerge.Focus();
                        dgMerge.SelectedIndex = rowIndex;
                        dgMerge.CurrentCell = dgMerge.GetCell(rowIndex, dgMerge.Columns["WIPQTY"].Index);
                    }, "800");
                }
            }

            txtLotID.Focus();
        }

        /// <summary>
        /// LoadedCellPresenter
        /// </summary>
        private void dgInfo_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Index == dg.Rows.Count -1)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                }

                if (e.Cell.Column.Name == "WIPQTY")
                {
                    e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(70);
                }

            }));
        }


        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (dgMerge == null || dgMerge.Rows.Count < 1)
                            return;

                        Button dg = sender as Button;
                        if (dg != null &&
                            dg.DataContext != null &&
                            (dg.DataContext as DataRowView).Row != null)
                        {
                            dgMerge.CommittedEdit -= dgMerge_CommittedEdit;

                            DataRow dtRow = (dg.DataContext as DataRowView).Row;

                            DataTable dt = DataTableConverter.Convert(dgMerge.ItemsSource);

                            DataRow[] dr = dt.Select("LOTID = '" + Util.NVC(dtRow["LOTID"]) + "'");

                            if (dr.Length > 0)
                            {
                                dt.Rows.Remove(dr[0]);
                            }
                            dt.AcceptChanges();

                            Util.GridSetData(dgMerge, dt, FrameOperation, false);

                            dgMerge.CommittedEdit += dgMerge_CommittedEdit;

                            ////////////////////////////////////////////////////////////////////////////////////////////////////////
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                List<double> listQty = dt.AsEnumerable().Select(row => double.Parse(Util.NVC(row["WIPQTY"]))).ToList();
                                txtTotQty.Text = listQty.Sum().ToString("#,###");
                            }
                            else
                            {
                                txtTotQty.Text = "0";
                            }
                        }

                        // 수량 합산
                        SumWipQty();

                        if (dgMerge.Rows.Count == 0)
                        {
                            InitializeControls();
                        }

                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLotID_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtLotID == null) return;
                InputMethod.SetPreferredImeConversionMode(txtLotID, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]
        private void GetLotInfo()
        {
            try
            {
                if (txtLotID.Text.Trim().Equals("")) return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = txtLotID.Text;

                if (Process.CPROD.Equals(_Procid))
                    newRow["WIPSTAT"] = null;
                else
                    newRow["WIPSTAT"] = "WAIT";

                inTable.Rows.Add(newRow);

                DataSet inDataSet = new DataSet();
                inDataSet.Tables.Add(inTable);

                new ClientProxy().ExecuteService_Multi("BR_PRD_SEL_MERGE_LOT_INFO_L", "INDATA", "OUTDATA", (bizResult, ex) =>
                {
                    try
                    {
                        txtLotID.Text = "";

                        if (ex != null)
                        {
                            Util.MessageException(ex, result =>
                            {
                                txtLotID.Text = "";
                                txtLotID.Focus();
                            });
                            return;
                        }

                        AddLotList(bizResult.Tables["OUTDATA"]);
                    }
                    catch (Exception ex1)
                    {
                        Util.MessageException(ex1);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();

                Util.MessageException(ex, result =>
                {
                    txtLotID.Text = "";
                    txtLotID.Focus();
                });
            }
        }

        private void MergeProcess()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");

                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("NOTE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataTable inFrom = inDataSet.Tables.Add("IN_FROMLOT");

                inFrom.Columns.Add("FROM_LOTID", typeof(string));

                DataTable inABNORMLOT = inDataSet.Tables.Add("IN_ABNORMLOT");

                inABNORMLOT.Columns.Add("ABNORM_LOTID", typeof(string));
                inABNORMLOT.Columns.Add("WIPQTY", typeof(decimal));

                //////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMerge.Rows[_util.GetDataGridCheckFirstRowIndex(dgMerge, "CHK")].DataItem, "LOTID"));
                newRow["NOTE"] = "";
                newRow["USERID"] = LoginInfo.USERID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;

                inTable.Rows.Add(newRow);

                for (int i = 0; i < dgMerge.Rows.Count; i++)
                {
                    if (_util.GetDataGridCheckValue(dgMerge, "CHK", i)) continue;

                    newRow = null;

                    newRow = inFrom.NewRow();

                    newRow["FROM_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMerge.Rows[i].DataItem, "LOTID"));

                    inFrom.Rows.Add(newRow);
                }

                // 불확실 Lot 
                DataRow[] drABNORMLOT = DataTableConverter.Convert(dgMerge.ItemsSource).Select("ABNORM_QTY_FLAG = 'Y'");
                foreach(DataRow dr in drABNORMLOT)
                {
                    newRow = null;
                    newRow = inABNORMLOT.NewRow();
                    newRow["ABNORM_LOTID"] = dr["LOTID"];
                    newRow["WIPQTY"] = dr["WIPQTY"];
                    inABNORMLOT.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MERGE_INOUT_LOT_ASSY_L", "INDATA,IN_FROMLOT,IN_ABNORMLOT", null, (bizResult, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        // 정상처리되었습니다.
                        Util.MessageValidation("SFU1275", result =>
                        {
                            InitializeControls();
                        });
                    }
                    catch (Exception ex1)
                    {
                        Util.MessageException(ex1);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
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

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        private void AddLotList(DataTable dtRslt)
        {
            try
            {
                if (dtRslt == null) return;

                if (dtRslt.Rows.Count < 1)
                {
                    Util.MessageInfo("SFU1386");   // LOT정보가 없습니다.
                    return;
                }

                //// 재생 매거진은 잔량 병합이 안됩니다.
                //if (Util.NVC(dtRslt.Rows[0]["MGZN_RECONF_FLAG"]).Equals("Y"))
                //{
                //    Util.MessageValidation("SFU5112", result =>
                //    {
                //        txtLotID.Text = "";
                //        txtLotID.Focus();
                //    });

                //    return;
                //}

                DataTable dtList = DataTableConverter.Convert(dgMerge.ItemsSource);

                if (dtList.Rows.Count > 0)
                {
                    // ScanValue 체크
                    if (!ValidationScanValue(dtRslt.Rows[0]))
                    {
                        return;
                    }
                }

                double dQty = 0;
                double.TryParse(Util.NVC(dtRslt.Rows[0]["WIPQTY"]), out dQty);

                DataRow dtRow = dtList.NewRow();
                dtRow["CHK"] = 0;
                dtRow["LOTID"] = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                dtRow["CSTID"] = Util.NVC(dtRslt.Rows[0]["CSTID"]);
                dtRow["WIPQTY"] = dQty;
                dtRow["PRODID"] = Util.NVC(dtRslt.Rows[0]["PRODID"]);
                dtRow["PRODNAME"] = Util.NVC(dtRslt.Rows[0]["PRODNAME"]);
                dtRow["PROCID"] = Util.NVC(dtRslt.Rows[0]["PROCID"]);
                dtRow["PROCNAME"] = Util.NVC(dtRslt.Rows[0]["PROCNAME"]);
                dtRow["WIPSTAT"] = Util.NVC(dtRslt.Rows[0]["WIPSTAT"]);
                dtRow["WIPSNAME"] = Util.NVC(dtRslt.Rows[0]["WIPSNAME"]);
                dtRow["EQSGID"] = Util.NVC(dtRslt.Rows[0]["EQSGID"]);
                dtRow["EQSGNAME"] = Util.NVC(dtRslt.Rows[0]["EQSGNAME"]);
                dtRow["ABNORM_QTY_FLAG"] = Util.NVC(dtRslt.Rows[0]["ABNORM_QTY_FLAG"]);
                dtRow["MGZN_RECONF_FLAG"] = Util.NVC(dtRslt.Rows[0]["MGZN_RECONF_FLAG"]);
                dtList.Rows.Add(dtRow);
                dtList.AcceptChanges();

                dgMerge.CommittedEdit -= dgMerge_CommittedEdit;

                Util.GridSetData(dgMerge, dtList, FrameOperation, true);

                dgMerge.CommittedEdit += dgMerge_CommittedEdit;

                // 첫번째(to Lot) 스캔인 경우
                if (dtList.Rows.Count == 1)
                {
                    bool bOK = double.TryParse(Util.NVC(dtRslt.Rows[0]["MAX_LOAD_QTY"]), out _max_LOAD_QTY);

                    if (bOK)
                        txtTotQty.Text = "0/" + _max_LOAD_QTY.ToString();
                    else
                        txtTotQty.Text = "0/0";

                    txtPrjtName.Text = Util.NVC(dtRslt.Rows[0]["PRJT_NAME"]);
                    txtPrdtClssCode.Text = Util.NVC(dtRslt.Rows[0]["PRDT_CLSS_CODE"]);

                    DataTableConverter.SetValue(dgMerge.Rows[0].DataItem, "CHK", true);
                }

                // 정확한 잔량을 알 수 없으므로, 적재 수량을 직접 입력 바랍니다.
                if (Util.NVC(dtRslt.Rows[0]["ABNORM_QTY_FLAG"]).Equals("Y"))
                {
                    Util.MessageValidation("SFU5109", result =>
                    {
                        //row 색 바꾸기
                        dgMerge.Focus();
                        dgMerge.SelectedIndex = dgMerge.Rows.Count - 1;
                        dgMerge.CurrentCell = dgMerge.GetCell(dgMerge.Rows.Count - 1, dgMerge.Columns["WIPQTY"].Index);
                    });
                }

                SumWipQty();

                // Scroll
                dgMerge.CurrentCell = dgMerge.GetCell(dgMerge.Rows.Count - 1, dgMerge.Columns["CHK"].Index);
                dgMerge.ScrollIntoView(dgMerge.Rows.Count - 1, dgMerge.Columns["CHK"].Index);

                txtLotID.Text = "";
                txtLotID.Focus();
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                Util.MessageValidation("SFU1386", result =>
                {
                    txtLotID.Text = "";
                    txtLotID.Focus();
                });

                return;
            }
        }

        private void SumWipQty()
        {
            DataTable dt = DataTableConverter.Convert(dgMerge.ItemsSource);

            if (dt == null || dt.Rows.Count == 0)
            {
                txtTotQty.Text = "0/" + _max_LOAD_QTY.ToString();
            }
            else
            {
                List<double> listQty = dt.AsEnumerable().Select(row => double.Parse(Util.NVC(row["WIPQTY"]))).ToList();
                txtTotQty.Text = listQty.Sum().ToString("###0") + "/" + _max_LOAD_QTY.ToString();
            }

            txtScanCount.Text = dgMerge.Rows.Count.ToString();
        }

        private bool ScanMaxQtyCheck(double ScanQty)
        {
            // 최대 수량 체크
            string[] QtySplit = txtTotQty.Text.Split('/');
            double SumQty = 0;
            double.TryParse(Util.NVC(QtySplit[0]), out SumQty);

            // 최대수량이 0이면 체크 제외
            if (_max_LOAD_QTY == 0)
                return false;
            else
                return SumQty + ScanQty > _max_LOAD_QTY;
        }

        #endregion

        #region [Validation]
        private bool ValidationScanValue(DataRow dr)
        {
            bool bRet = false;

            DataTable dtTmp = DataTableConverter.Convert(dgMerge.ItemsSource);

            DataRow[] dtRows = dtTmp.Select("LOTID = '" + Util.NVC(dr["LOTID"]) + "'");

            if (dtRows.Length > 0)
            {
                // 이미 바코드 스캔 완료하였습니다.
                Util.MessageValidation("SFU5114", result =>
                {
                    txtLotID.Text = "";
                    txtLotID.Focus();
                });
                return bRet;
            }

            // 동일 공정 체크
            if (Util.NVC(dtTmp.Rows[0]["PROCID"]) != Util.NVC(dr["PROCID"]))
            {
                // 동일 공정이 아닙니다.
                Util.MessageValidation("SFU3600", result =>
                {
                    txtLotID.Text = "";
                    txtLotID.Focus();
                });
                return bRet;
            }

            // 동일 제품 체크
            if (Util.NVC(dtTmp.Rows[0]["PRODID"]) != Util.NVC(dr["PRODID"]))
            {
                // 동일 모델의 반제품이 아닙니다.
                Util.MessageValidation("SFU5105", result =>
                {
                    txtLotID.Text = "";
                    txtLotID.Focus();
                });
                return bRet;
            }

            // 대기 상태 체크
            DataRow[] dtRow = dtTmp.Select("WIPSTAT <> 'WAIT'");
            if (Util.NVC(dr["WIPSTAT"]) != "WAIT")
            {
                // 대기 상태의 Lot만 병합이 가능합니다. 
                Util.MessageValidation("SFU5106", result =>
                {
                    txtLotID.Text = "";
                    txtLotID.Focus();
                });
                return bRet;
            }

            //// 동일 라인 체크
            //if (Util.NVC(dtTmp.Rows[0]["EQSGID"]) != Util.NVC(dr["EQSGID"]))
            //{
            //    // %1 Line의 LOT만 병합이 가능 합니다.
            //    Util.MessageInfo("SFU5107", result =>
            //    {
            //        txtLotID.Text = "";
            //        txtLotID.Focus();
            //    }, Util.NVC(dtTmp.Rows[0]["EQSGID"]));
            //    return bRet;
            //}

            if (Util.NVC(dtTmp.Rows[0]["MGZN_RECONF_FLAG"]) != Util.NVC(dr["MGZN_RECONF_FLAG"]))
            {
                // 일반 매거진과 재작업 매거진은 병합 할수 없습니다. 
                Util.MessageInfo("SFU5113", result =>
                {
                    txtLotID.Text = "";
                    txtLotID.Focus();
                });
                return bRet;
            }

            // 최대 수량 체크
            double ScanQty = 0;
            double.TryParse(Util.NVC(dr["WIPQTY"]), out ScanQty);

            if (ScanMaxQtyCheck(ScanQty))
            {
                //  최대 수량을 초과할 수 없습니다.
                Util.MessageValidation("SFU5110", result =>
                {
                    txtLotID.Text = "";
                    txtLotID.Focus();
                });
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private bool CanSave()
        {
            bool bRet = false;

            DataTable dtTmp = DataTableConverter.Convert(dgMerge.ItemsSource);

            if (dtTmp == null || dtTmp.Rows.Count < 1)
            {
                Util.MessageValidation("SFU3708");  // Merge할 Lot이 없습니다.
                return bRet;
            }
            else if (dtTmp.Rows.Count == 1)
            {
                Util.MessageValidation("SFU3709");  // 최소 Lot이 2개 있어야 Merge할 수 있습니다.
                return bRet;
            }

            if (_util.GetDataGridCheckFirstRowIndex(dgMerge, "CHK") < 0)
            {
                Util.MessageValidation("SFU3707");  // Merge Lot을 선택하세요.
                return bRet;
            }

            // 비정상 수량인 경우 수량 변경 여부 체크
            DataRow[] dtRow3 = dtTmp.Select("ABNORM_QTY_FLAG = 'Y' and WIPQTY = 0");
            if (dtRow3.Length > 0)
            {
                // 정확한 잔량을 알 수 없으므로, 적재 수량을 직접 입력 바랍니다.
                Util.MessageValidation("SFU5109");
                return bRet;
            }

            dtRow3 = dtTmp.Select("ABNORM_QTY_FLAG = 'Y' and WIPQTY > 800");
            if (dtRow3.Length > 0)
            {
                // 비정상 수량 수정시 최대 [%1]개를 초과할 수 없습니다.
                Util.MessageValidation("SFU8125", "800");
                return bRet;
            }

            if (ScanMaxQtyCheck(0))
            {
                //  최대 수량을 초과할 수 없습니다.
                Util.MessageValidation("SFU5110", result =>
                {
                    txtLotID.Text = "";
                    txtLotID.Focus();
                });
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        #endregion

        #endregion

    }
}
