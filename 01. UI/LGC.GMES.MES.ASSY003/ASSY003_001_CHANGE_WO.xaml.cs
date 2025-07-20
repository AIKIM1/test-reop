/*************************************************************************************
 Created Date : 2018.01.11
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Notching 공정진척 화면 - 다모델 인경우 생산LOT W/O변경
--------------------------------------------------------------------------------------
 [Change History]
  2018.01.11  INS 김동일K : Initial Created.

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_001_CHANGE_WO.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_001_CHANGE_WO : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _ProcID = string.Empty;
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _InQty = string.Empty;
        private string _GoodQty = string.Empty;
        private string _DfctQty = string.Empty;
        private string _PrProdID = string.Empty;

        private bool bLoaded = false;
        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
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

        public ASSY003_001_CHANGE_WO()
        {
            InitializeComponent();
        }

        private void InitializeCombo()
        {
            DataTable dtTemp = GetEquipmentSegmentCombo();

            if (dtTemp != null)
            {
                cboEquipmentSegment.SelectedValueChanged -= cboEquipmentSegment_SelectedValueChanged;
                cboEquipmentSegment.DisplayMemberPath = "CBO_NAME";
                cboEquipmentSegment.SelectedValuePath = "CBO_CODE";

                cboEquipmentSegment.ItemsSource = dtTemp.Copy().AsDataView();
                cboEquipmentSegment.SelectedIndex = 0;
                cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            }
        }

        private void InitializeGridColumns()
        {
            if (dgWorkOrder == null)
                return;

            /*
             * C/Roll, S/Roll, Lane수 적용 공정.
             *     C/ROLL = PLAN_QTY(S/ROLL) / LANE_QTY
             * E2000  - TOP_COATING
             * E2300  - INS_COATING
             * S2000  - SRS_COATING
             * E2500  - HALF_SLITTING
             * E3000  - ROLL_PRESSING
             * E3500  - TAPING
             * E3800  - REWINDER
             * E3900  - BACK_WINDER
             */
            if (_ProcID.Equals(Process.TOP_COATING) ||
                _ProcID.Equals(Process.INS_COATING) ||
                _ProcID.Equals(Process.SRS_COATING) ||
                _ProcID.Equals(Process.HALF_SLITTING) ||
                _ProcID.Equals(Process.ROLL_PRESSING) ||
                _ProcID.Equals(Process.TAPING) ||
                _ProcID.Equals(Process.REWINDER) ||
                _ProcID.Equals(Process.BACK_WINDER))
            {
                if (dgWorkOrder.Columns.Contains("C_ROLL_QTY"))
                {
                    dgWorkOrder.Columns["C_ROLL_QTY"].Visibility = Visibility.Visible;
                }

                if (dgWorkOrder.Columns.Contains("S_ROLL_QTY"))
                {
                    dgWorkOrder.Columns["S_ROLL_QTY"].Visibility = Visibility.Visible;
                }

                if (dgWorkOrder.Columns.Contains("LANE_QTY"))
                {
                    dgWorkOrder.Columns["LANE_QTY"].Visibility = Visibility.Visible;
                }

                if (dgWorkOrder.Columns.Contains("INPUT_QTY"))
                {
                    dgWorkOrder.Columns["INPUT_QTY"].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (dgWorkOrder.Columns.Contains("C_ROLL_QTY"))
                {
                    dgWorkOrder.Columns["C_ROLL_QTY"].Visibility = Visibility.Collapsed;
                }

                if (dgWorkOrder.Columns.Contains("S_ROLL_QTY"))
                {
                    dgWorkOrder.Columns["S_ROLL_QTY"].Visibility = Visibility.Collapsed;
                }

                if (dgWorkOrder.Columns.Contains("LANE_QTY"))
                {
                    dgWorkOrder.Columns["LANE_QTY"].Visibility = Visibility.Collapsed;
                }

                if (dgWorkOrder.Columns.Contains("INPUT_QTY"))
                {
                    dgWorkOrder.Columns["INPUT_QTY"].Visibility = Visibility.Visible;
                }
            }

            // 조립 공정인 경우 버전 컬럼 HIDDEN.
            if (_ProcID.Equals(Process.NOTCHING) ||
                _ProcID.Equals(Process.LAMINATION) ||
                _ProcID.Equals(Process.STACKING_FOLDING) ||
                _ProcID.Equals(Process.PACKAGING) ||
                _ProcID.Equals(Process.WINDING) ||
                _ProcID.Equals(Process.ASSEMBLY) ||
                _ProcID.Equals(Process.WASHING)
                )
            {
                if (dgWorkOrder.Columns.Contains("PROD_VER_CODE"))
                {
                    dgWorkOrder.Columns["PROD_VER_CODE"].Visibility = Visibility.Collapsed;
                }
            }

            // 패키지 공정인 경우만 모델랏 정보 표시
            if (dgWorkOrder.Columns.Contains("MDLLOT_ID"))
            {
                if (_ProcID.Equals(Process.PACKAGING) || _ProcID.Equals(Process.WINDING) || _ProcID.Equals(Process.ASSEMBLY))
                {
                    dgWorkOrder.Columns["MDLLOT_ID"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgWorkOrder.Columns["MDLLOT_ID"].Visibility = Visibility.Collapsed;
                }
            }

            // 라미 공정일 경우 Cell Type (CLSS_NAME : 분류명) 컬럼 표시 -> 극성 컬럼 Hidden
            if (_ProcID.Equals(Process.LAMINATION))
            {
                if (dgWorkOrder.Columns.Contains("ELECTYPE")) dgWorkOrder.Columns["ELECTYPE"].Visibility = Visibility.Collapsed;
                if (dgWorkOrder.Columns.Contains("CLSS_ID")) dgWorkOrder.Columns["CLSS_ID"].Visibility = Visibility.Collapsed;
                if (dgWorkOrder.Columns.Contains("CLSS_NAME")) dgWorkOrder.Columns["CLSS_NAME"].Visibility = Visibility.Visible;

                // 남경 라미인 경우 CLSS_NAME 대신에 PRODNAME으로 표시 처리.
                if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
                {
                    if (dgWorkOrder.Columns.Contains("CLSS_NAME")) dgWorkOrder.Columns["CLSS_NAME"].Visibility = Visibility.Collapsed;
                    if (dgWorkOrder.Columns.Contains("PRODNAME")) dgWorkOrder.Columns["PRODNAME"].Visibility = Visibility.Visible;
                }
                else
                {
                    if (dgWorkOrder.Columns.Contains("CLSS_NAME")) dgWorkOrder.Columns["CLSS_NAME"].Visibility = Visibility.Visible;
                    if (dgWorkOrder.Columns.Contains("PRODNAME")) dgWorkOrder.Columns["PRODNAME"].Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (dgWorkOrder.Columns.Contains("ELECTYPE")) dgWorkOrder.Columns["ELECTYPE"].Visibility = Visibility.Visible;
                if (dgWorkOrder.Columns.Contains("CLSS_ID")) dgWorkOrder.Columns["CLSS_ID"].Visibility = Visibility.Collapsed;
                if (dgWorkOrder.Columns.Contains("CLSS_NAME")) dgWorkOrder.Columns["CLSS_NAME"].Visibility = Visibility.Collapsed;
                if (dgWorkOrder.Columns.Contains("PRODNAME")) dgWorkOrder.Columns["PRODNAME"].Visibility = Visibility.Collapsed;
            }

            // 노칭을 제외한 조립 공정 극성 컬럼 Hidden
            if (_ProcID.Equals(Process.STACKING_FOLDING) ||
                _ProcID.Equals(Process.SRC) ||
                _ProcID.Equals(Process.STP) ||
                _ProcID.Equals(Process.SSC_BICELL) ||
                _ProcID.Equals(Process.SSC_FOLDED_BICELL) ||
                _ProcID.Equals(Process.PACKAGING) ||
                _ProcID.Equals(Process.WINDING) ||
                _ProcID.Equals(Process.ASSEMBLY) ||
                _ProcID.Equals(Process.WASHING)
                )
            {
                if (dgWorkOrder.Columns.Contains("ELECTYPE")) dgWorkOrder.Columns["ELECTYPE"].Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Event

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            SetChangeDatePlan();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 8)
            {
                _ProcID = Util.NVC(tmps[0]);
                _LineID = Util.NVC(tmps[1]);
                _EqptID = Util.NVC(tmps[2]);
                _LotID = Util.NVC(tmps[3]);
                _WipSeq = Util.NVC(tmps[4]);
                _InQty = Util.NVC(tmps[5]);
                _GoodQty = Util.NVC(tmps[6]);
                _DfctQty = Util.NVC(tmps[7]);
            }
            else
            {
                _ProcID = "";
                _LineID = "";
                _EqptID = "";
                _LotID = "";
                _WipSeq = "";
                _InQty = "0";
                _GoodQty = "0";
                _DfctQty = "0";
            }
            ApplyPermissions();

            InitializeCombo();
            InitializeGridColumns();
            SetGridData(dgWorkOrder);

            dtpDateFrom.Tag = "CHANGE";
            dtpDateTo.Tag = "CHANGE";

            ClearConstols();
            GetDefaultInfo();
            GetWorkOrder();

            double dTmp = 0;
            double.TryParse(_InQty, out dTmp);
            txtInQty.Text = dTmp.ToString();

            dTmp = 0;
            double.TryParse(_GoodQty, out dTmp);
            txtGoodQty.Text = dTmp.ToString();

            dTmp = 0;
            double.TryParse(_DfctQty, out dTmp);
            txtDfctQty.Text = dTmp.ToString();

            bLoaded = true;
        }

        private void btnChange_Clicked(object sender, RoutedEventArgs e)
        {
            if (!CanChange())
                return;

            Util.MessageConfirm("SFU2875", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ChangeWorkOrder();
                }
            });
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void txtInQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtInQty.Text, 0))
                {
                    txtInQty.Text = "";
                    return;
                }

                double dTmpIn = 0;
                double dTmpDfct = 0;
                double.TryParse(txtInQty.Text, out dTmpIn);
                double.TryParse(txtDfctQty.Text, out dTmpDfct);

                txtGoodQty.Text = (dTmpIn - dTmpDfct).ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtInQty_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtInQty.Text, 0))
                {
                    txtInQty.Text = "";
                    return;
                }

                double dTmpIn = 0;
                double dTmpDfct = 0;
                double.TryParse(txtInQty.Text, out dTmpIn);
                double.TryParse(txtDfctQty.Text, out dTmpDfct);

                txtGoodQty.Text = (dTmpIn - dTmpDfct).ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetWorkOrder();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgWorkOrderChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            // 부모 조회 없으므로 로직 수정..
            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
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
                dgWorkOrder.SelectedIndex = idx;
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (string.Equals(dtPik.Tag, "CHANGE"))
                {
                    dtPik.Tag = null;
                    return;
                }
                
                // BASETIME 기준설정
                DateTime currDate = DateTime.Now;
                DateTime baseDate = DateTime.Now;
                string sCurrTime = string.Empty;
                string sBaseTime = string.Empty;

                GetChangeDatePlan(out currDate, out sCurrTime, out sBaseTime);

                if (Util.NVC_Decimal(sCurrTime) - Util.NVC_Decimal(sBaseTime) < 0)
                    baseDate = currDate.AddDays(-1);

                // W/O 공정인 경우에만 체크.
                //if (_Process_Plan_Mngt_Type_Code.Equals("WO"))
                //{
                    if (Convert.ToDecimal(baseDate.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                    //if (Convert.ToDecimal(baseDate.ToString("yyyyMM") + "01") > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                    {
                        dtPik.Text = baseDate.ToLongDateString();
                        dtPik.SelectedDateTime = baseDate;
                        Util.MessageValidation("SFU1738");  //오늘 이전 날짜는 선택할 수 없습니다.
                        return;
                    }
                //}
                //else
                //{
                //    //if (Convert.ToDecimal(baseDate.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                //    if (Convert.ToDecimal(baseDate.ToString("yyyyMM") + "01") > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                //    {
                //        dtPik.Text = baseDate.ToLongDateString();
                //        dtPik.SelectedDateTime = baseDate;
                //        Util.MessageValidation("SFU1738");  //오늘 이전 날짜는 선택할 수 없습니다.
                //        return;
                //    }
                //}
                
                if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtPik.Text = baseDate.ToLongDateString();
                    dtPik.SelectedDateTime = baseDate;
                    Util.MessageValidation("SFU3231");  // 종료시간이 시작시간보다 이전입니다
                    //e.Handled = false;
                    return;
                }

                GetWorkOrder();
            }));
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                    return;

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (string.Equals(dtPik.Tag, "CHANGE"))
                {
                    dtPik.Tag = null;
                    return;
                }
                
                // BASETIME 기준설정
                DateTime currDate = DateTime.Now;
                DateTime baseDate = DateTime.Now;
                string sCurrTime = string.Empty;
                string sBaseTime = string.Empty;

                GetChangeDatePlan(out currDate, out sCurrTime, out sBaseTime);

                if (Util.NVC_Decimal(sCurrTime) - Util.NVC_Decimal(sBaseTime) < 0)
                    baseDate = currDate.AddDays(-1);

                if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    baseDate = dtpDateFrom.SelectedDateTime;

                    dtPik.Text = baseDate.ToLongDateString();
                    dtPik.SelectedDateTime = baseDate;
                    Util.MessageValidation("SFU1698");  //시작일자 이전 날짜는 선택할 수 없습니다.
                    //e.Handled = false;
                    return;
                }
                
                GetWorkOrder();
            }));
        }

        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                GetWorkOrder();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                GetWorkOrder();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed &&
                     (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                     (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                     (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    chkAll.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]

        private void GetDefaultInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["LOTID"] = _LotID;
                newRow["WIPSEQ"] = _WipSeq;
                newRow["EQPTID"] = _EqptID;


                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CHILDLOT_INFO_FOR_WO_CHG", "INDATA", "OUTDATA", inTable);

                if (searchResult?.Rows.Count > 0)
                {
                    txtLotID.Text = Util.NVC(searchResult.Rows[0]["LOTID"]);
                    txtWorkorder.Text = Util.NVC(searchResult.Rows[0]["WOID"]);
                    txtMkTypeName.Text = Util.NVC(searchResult.Rows[0]["MKT_TYPE_NAME"]);
                    txtMkTypeCode.Text = Util.NVC(searchResult.Rows[0]["MKT_TYPE_CODE"]);
                    txtProdID.Text = Util.NVC(searchResult.Rows[0]["PRODID"]);
                    txtGoodQty.Text = Util.NVC(searchResult.Rows[0]["GOODQTY"]).Equals("") ? "0" : double.Parse(Util.NVC(searchResult.Rows[0]["GOODQTY"])).ToString();
                    txtDfctQty.Text = Util.NVC(searchResult.Rows[0]["DFCTQTY"]).Equals("") ? "0" : double.Parse(Util.NVC(searchResult.Rows[0]["DFCTQTY"])).ToString();
                    txtInQty.Text = Util.NVC(searchResult.Rows[0]["INPUTQTY"]).Equals("") ? "0" : double.Parse(Util.NVC(searchResult.Rows[0]["INPUTQTY"])).ToString();
                    _PrProdID = Util.NVC(searchResult.Rows[0]["PR_PRODID"]);
                    rtxRemark.AppendText("");
                }
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

        private void GetWorkOrder()
        {
            try
            {
                if (!bLoaded) return;
                ShowLoadingIndicator();

                //string sCaldDate = GetCalDate();

                string bizRuleName = "DA_PRD_SEL_WORKORDER_LIST_FOR_CHG_WO_NT";

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("STDT", typeof(string));
                inTable.Columns.Add("EDDT", typeof(string));
                inTable.Columns.Add("COAT_SIDE_TYPE", typeof(string));
                inTable.Columns.Add("OTHER_EQSGID", typeof(string));
                inTable.Columns.Add("PROC_EQPT_FLAG", typeof(string));
                inTable.Columns.Add("MTRLID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));


                DataRow searchCondition = inTable.NewRow();
                searchCondition["LANGID"] = LoginInfo.LANGID;
                searchCondition["PROCID"] = _ProcID;
                searchCondition["EQPTID"] = _EqptID;
                searchCondition["STDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["EDDT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                searchCondition["MTRLID"] = (bool)chkAll.IsChecked ? null : (_PrProdID.Equals("") ? null : _PrProdID);
                searchCondition["PRODID"] = (bool)chkAll.IsChecked ? null : txtProdID.Text;

                if (cboEquipmentSegment != null && cboEquipmentSegment.Items.Count > 0 &&
                    !Util.NVC(cboEquipmentSegment.SelectedValue).Equals(""))
                {
                    searchCondition["OTHER_EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                    searchCondition["EQSGID"] = "";
                    searchCondition["PROC_EQPT_FLAG"] = "LINE";
                }
                else
                {
                    searchCondition["OTHER_EQSGID"] = "";
                    searchCondition["EQSGID"] = _LineID;
                    searchCondition["PROC_EQPT_FLAG"] = GetFpPlanGnrtBasCode();
                }

                inTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                //if (dtRslt?.Rows.Count > 0)
                //{
                //    DataRow[] drTmp = dtRslt.Select("PRODID <> '" + txtProdID.Text + "'");
                //    if (drTmp.Length > 0)
                //        dtRslt = drTmp.CopyToDataTable();
                //    else
                //        dtRslt.Clear();
                //}

                Util.GridSetData(dgWorkOrder, dtRslt, FrameOperation, true);
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

        private void ChangeWorkOrder()
        {
            try
            {
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK");

                ShowLoadingIndicator();
                
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WOID", typeof(string));
                inTable.Columns.Add("WO_DETL_ID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = _LotID;
                newRow["WOID"] = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WOID"));
                newRow["WO_DETL_ID"] = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "WO_DETL_ID"));
                newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgWorkOrder.Rows[idx].DataItem, "PRODID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
               
                new ClientProxy().ExecuteService("BR_PRD_REG_MODIFY_LOT_WOID_NT_S", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
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

        private DataTable GetEquipmentSegmentCombo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = _ProcID;
                dr["EQPTID"] = _EqptID;
                RQSTDT.Rows.Add(dr);
                
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_FLOOR", "RQSTDT", "RSLTDT", RQSTDT);

                DataTable dtTemp = null;
                
                if (dtResult.Rows.Count > 0)
                {
                    dtTemp = dtResult;
                }
                else
                {
                    dtTemp = new DataTable();

                    dtTemp.Columns.Add("CBO_NAME", typeof(string));
                    dtTemp.Columns.Add("CBO_CODE", typeof(string));
                }

                DataRow dr2 = dtTemp.NewRow();
                dr2["CBO_NAME"] = "- " + ObjectDic.Instance.GetObjectName("타라인") + " -";
                dr2["CBO_CODE"] = "";
                dtTemp.Rows.InsertAt(dr2, 0);

                return dtTemp;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                return null;
            }
        }

        private string GetFpPlanGnrtBasCode()
        {
            try
            {
                string sPlanType = "";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = _ProcID;
                dr["EQSGID"] = _LineID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FP_PLAN_GNRT_BAS_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Columns.Contains("FP_PLAN_GNRT_BAS_CODE"))
                {
                    if (Util.NVC(dtResult.Rows[0]["FP_PLAN_GNRT_BAS_CODE"]).Equals("E"))
                    {
                        sPlanType = "EQPT";
                        dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        sPlanType = "LINE";
                        dgWorkOrder.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;
                    }
                }

                return sPlanType;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "LINE";
            }
        }

        private string GetCalDate()
        {
            try
            {
                string sCalDate = "";

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = _EqptID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CALDATE_EQPTID", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0 && dtResult.Columns.Contains("CALDATE_YMD"))
                {
                    sCalDate = Util.NVC(dtResult.Rows[0]["CALDATE_YMD"]);
                }
                else
                {
                    sCalDate = DateTime.Now.ToString("yyyyMMdd");
                }

                return sCalDate;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return System.DateTime.Now.ToString("yyyyMMdd");
            }
        }

        private void GetChangeDatePlan(out DateTime currDate, out string sCurrTime, out string sBaseTime)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("AREAID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            IndataTable.Rows.Add(Indata);

            DataTable dateDt = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "INDATA", "RSLTDT", IndataTable);

            currDate = GetCurrentTime();
            sCurrTime = currDate.ToString("HHmmss");
            sBaseTime = string.IsNullOrEmpty(Util.NVC(dateDt.Rows[0]["S02"])) ? "000000" : Util.NVC(dateDt.Rows[0]["S02"]);
        }

        private DateTime GetCurrentTime()
        {
            try
            {
                DataTable dt = new ClientProxy().ExecuteServiceSync("BR_CUS_GET_SYSTIME", null, "OUTDATA", null);
                return (DateTime)dt.Rows[0]["SYSTIME"];
            }
            catch (Exception ex) { }

            return DateTime.Now;
        }

        private void SetChangeDatePlan(bool isInitFlag = true)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("AREAID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
            IndataTable.Rows.Add(Indata);

            DataTable dateDt = new ClientProxy().ExecuteServiceSync("CUS_SEL_AREAATTR", "INDATA", "RSLTDT", IndataTable);

            DateTime currDate = GetCurrentTime();
            string currTime = currDate.ToString("HHmmss");
            string baseTime = string.IsNullOrEmpty(Util.NVC(dateDt.Rows[0]["S02"])) ? "000000" : Util.NVC(dateDt.Rows[0]["S02"]);

            if (isInitFlag)
            {
                if (Util.NVC_Decimal(currTime) - Util.NVC_Decimal(baseTime) < 0)
                {
                    dtpDateFrom.SelectedDateTime = currDate.AddDays(-1);
                    dtpDateFrom.Tag = "CHANGE";

                    dtpDateTo.SelectedDateTime = currDate.AddDays(-1);
                    dtpDateTo.Tag = "CHANGE";
                }
            }
            else
            {
                if (Util.NVC_Decimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) < Util.NVC_Decimal(currDate.ToString("yyyyMMdd")) &&
                    Util.NVC_Decimal(currTime) - Util.NVC_Decimal(baseTime) > 0)
                {
                    dtpDateFrom.SelectedDateTime = currDate;
                    dtpDateFrom.Tag = "CHANGE";
                }

                if (Util.NVC_Decimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Util.NVC_Decimal(currDate.ToString("yyyyMMdd")) &&
                    Util.NVC_Decimal(currTime) - Util.NVC_Decimal(baseTime) > 0)
                {
                    dtpDateTo.SelectedDateTime = currDate;
                    dtpDateTo.Tag = "CHANGE";
                }
            }
        }
        #endregion

        #region [Validation]
        private bool CanChange()
        {
            bool bRet = false;
            
            //if (new TextRange(rtxRemark.Document.ContentStart, rtxRemark.Document.ContentEnd).Text.Trim().Equals(""))
            //{
            //    Util.MessageValidation("SFU1594");
            //    return bRet;
            //}

            //double dTmp = 0;
            //double.TryParse(txtInQty.Text, out dTmp);

            //if (dTmp < 1)
            //{
            //    Util.MessageValidation("SFU1609"); 
            //    return bRet;
            //}

            //double.TryParse(txtGoodQty.Text, out dTmp);

            //if (dTmp < 0)
            //{
            //    Util.MessageValidation("SFU1721");
            //    return bRet;
            //}


            if (_Util.GetDataGridCheckFirstRowIndex(dgWorkOrder, "CHK") < 0)
            {
                Util.MessageValidation("SFU1635");
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        #endregion

        #region [Func]

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnChange);

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

        private void ClearConstols()
        {
            try
            {
                txtLotID.Text = "";
                txtWorkorder.Text = "";
                txtMkTypeName.Text = "";
                txtMkTypeCode.Text = "";
                txtProdID.Text = "";
                txtGoodQty.Text = "";
                txtDfctQty.Text = "";
                txtInQty.Text = "";
                rtxRemark.Document.Blocks.Clear();

                Util.gridClear(dgWorkOrder);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetGridData(C1DataGrid dg)
        {
            dg.Columns["CHK"].DisplayIndex = 0;
            dg.Columns["EIO_WO_SEL_STAT"].DisplayIndex = 1;
            dg.Columns["PRJT_NAME"].DisplayIndex = 2;
            dg.Columns["PROD_VER_CODE"].DisplayIndex = 3;
            dg.Columns["WOID"].DisplayIndex = 4;
            dg.Columns["MDLLOT_ID"].DisplayIndex = 5;
            dg.Columns["PRODID"].DisplayIndex = 6;
            dg.Columns["MKT_TYPE_NAME"].DisplayIndex = 7;
            dg.Columns["CELL_3DTYPE"].DisplayIndex = 8;
            dg.Columns["PRODNAME"].DisplayIndex = 9;
            dg.Columns["ELECTYPE"].DisplayIndex = 10;

            dg.Columns["MODLID"].DisplayIndex = 11;
            dg.Columns["LOTYNAME"].DisplayIndex = 12;
            dg.Columns["INPUT_QTY"].DisplayIndex = 13;
            dg.Columns["C_ROLL_QTY"].DisplayIndex = 14;
            dg.Columns["S_ROLL_QTY"].DisplayIndex = 15;
            dg.Columns["LANE_QTY"].DisplayIndex = 16;
            dg.Columns["OUTQTY"].DisplayIndex = 17;
            dg.Columns["UNIT_CODE"].DisplayIndex = 18;
            dg.Columns["STRT_DTTM"].DisplayIndex = 19;
            dg.Columns["END_DTTM"].DisplayIndex = 20;

            dg.Columns["WO_STAT_NAME"].DisplayIndex = 21;
            dg.Columns["WO_STAT_CODE"].DisplayIndex = 22;
            dg.Columns["WO_DETL_ID"].DisplayIndex = 23;
            dg.Columns["EQSGID"].DisplayIndex = 24;
            dg.Columns["EQSGNAME"].DisplayIndex = 25;
            dg.Columns["EQPTID"].DisplayIndex = 26;
            dg.Columns["EQPTNAME"].DisplayIndex = 27;
            dg.Columns["CLSS_ID"].DisplayIndex = 28;
            dg.Columns["CLSS_NAME"].DisplayIndex = 29;
            dg.Columns["PLAN_TYPE_NAME"].DisplayIndex = 30;

            dg.Columns["PLAN_TYPE"].DisplayIndex = 31;
            dg.Columns["WOTYPE"].DisplayIndex = 32;
            dg.Columns["EIO_WO_DETL_ID"].DisplayIndex = 33;
            dg.Columns["PRDT_CLSS_CODE"].DisplayIndex = 34;
            dg.Columns["DEMAND_TYPE"].DisplayIndex = 35;

            dg.FrozenColumnCount = 7;

        }


        #endregion

        #endregion
        
    }
}
