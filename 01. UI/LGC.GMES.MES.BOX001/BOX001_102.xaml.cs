/*************************************************************************************
 Created Date : 2017.07.04
      Creator : 이슬아
   Decription : 포장 공정실적
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.04  이슬아 : Initial Created.
  2017.12.07  이상훈 : C20171128_42916 [CSR ID:3542916] [GMES] 반품확정 임의 Lot 등록
  2018.01.20  이상훈 : [20180120-01] 반품 확정 처리 시 반품유형 선택 저장 할 수 있도록 개선
  2018.04.27  이상훈   C20180417_64069 물류반품 팔레트 SOC 및 용량 grade 저장 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Globalization;
using System.Reflection;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media.Animation;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_100.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_102 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private Util _util = new Util();

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre1 = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre2 = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        C1.WPF.DataGrid.DataGridRowHeaderPresenter preReturn = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll1 = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };


        CheckBox chkAll2 = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        CheckBox chkAllReturn = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_102()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT);
            _combo.SetCombo(cboArea2, CommonCombo.ComboStatus.ALL, sCase: "cboArea");
            _combo.SetCombo(cboArea3, CommonCombo.ComboStatus.ALL, sCase: "cboArea");

            _combo.SetCombo(cboStat, CommonCombo.ComboStatus.ALL, sFilter: new string[] { "RETURN_RCV_ISS_STAT_CODE" }, sCase: "COMMCODE_WITHOUT_CODE");

            (dgLotInfo.Columns["PRDT_GRD_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtTypeCombo("VLTG_GRD_CODE"));
            (dgLotInfo.Columns["CAPA_GRD_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtCapaCombo("CAPA_GRD_CODE"));
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
        }

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

        #endregion

        #region Event

        #region [Main Window]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
        }


        private void dgReturn_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblShipping.Tag))
                    {
                        e.Cell.Presenter.Background = lblShipping.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblFinish.Tag))
                    {
                        e.Cell.Presenter.Background = lblFinish.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblCancel.Tag))
                    {
                        e.Cell.Presenter.Background = lblCancel.Background;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgReturnResultChoice_Checked(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        private void dgLotInfo_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name == "BOXID")
            {
                e.Cancel = true;
                return;
            }

            // 신규행만 수정/삭제 가능
            //if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "YN")) == "N")
            //{
            //    e.Cancel = true;
            //    return;
            //}
        }

        private void dgLotInfo_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            //string sLotID = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "LOTID"));
            //string sGrd = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "PRDT_GRD"));
            //string sGrdLevel = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "PRDT_GRD_LEVEL"));

            //DataTable dtResult = DataTableConverter.Convert(dgLotInfo.ItemsSource);
        }

        private void dgLotInfo_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            //string sRcvID = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ISS_RCV_ID"));
            //string sLotID = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOTID"));
            //string sGrdCode = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRDT_GRD_CODE"));

            DataTable dtResult = DataTableConverter.Convert(dgLotInfo.ItemsSource);
            if (e.Cell.Column.Name == "LOTQTY2")
            {
                txtReturnQty.Value = Util.NVC_Int(dtResult.Compute("SUM(LOTQTY2)", string.Empty));
                txtQty.Value = txtReqQty.Value - txtReturnQty.Value;
            }

            //if (e.Cell.Column.Name == "PRDT_GRD" || e.Cell.Column.Name == "PRDT_GRD_LEVEL")
            //{
            //    if (string.IsNullOrWhiteSpace(sGrd) || string.IsNullOrWhiteSpace(sGrdLevel))
            //    {
            //        if (string.IsNullOrWhiteSpace(Util.NVC(e.Cell.Value)))
            //        {
            //            dgLotInfo.GetCell(e.Cell.Row.Index, dgLotInfo.Columns["PRDT_GRD"].Index).Value = string.Empty;
            //            dgLotInfo.GetCell(e.Cell.Row.Index, dgLotInfo.Columns["PRDT_GRD_LEVEL"].Index).Value = string.Empty;
            //        }
            //        dgLotInfo.GetCell(e.Cell.Row.Index, dgLotInfo.Columns["PRDT_GRD_CODE"].Index).Value = "NA";
            //    }

            //    else
            //        dgLotInfo.GetCell(e.Cell.Row.Index, dgLotInfo.Columns["PRDT_GRD_CODE"].Index).Value = sGrd + "-" + sGrdLevel;
            //}


            //List<DataRow> drList = dtResult.Select("LOTID = '" + sLotID + "' AND PRDT_GRD = '" + sGrd + "' AND PRDT_GRD_LEVEL = '" + sGrdLevel + "'").ToList();
            //if (drList.Count > 1)
            //{
            //    //SFU3605	동일한 등급-레벨 [%1]이 존재합니다.
            //    Util.MessageValidation("SFU3605", new object[] { sLotID + " (" + sGrd + "-" + sGrdLevel + ")" });
            //    return;
            //}


            //List<DataRow> drList = dtResult.Select("ISS_RCV_ID = '" + sRcvID + "' AND LOTID = '" + sLotID + "' AND PRDT_GRD_CODE = '" + sGrdCode + "'").ToList();
            //if (drList.Count > 1)
            //{
            //    //SFU3605	동일한 등급-레벨 [%1]이 존재합니다.
            //    Util.MessageValidation("SFU3605", new object[] { sLotID + " (" + sGrdCode + ")" });
            //    return;
            //}
        }

        #region [Button]
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            int iRow = -1;
            List<int> row = _util.GetDataGridCheckRowIndex(dgLotInfo, "CHK");

            if (row.Count > 0)
                iRow = row[row.Count - 1];

            DataTable dtInfo = DataTableConverter.Convert(dgLotInfo.ItemsSource);

            if (dtInfo.Columns.Count == 0)
                return;
            
            DataView dv = cboFormReturnTypeCode.ItemsSource as DataView;
            DataTable dt = dv.ToTable();

            DataRow drInfo = dtInfo.NewRow();
            //drInfo["YN"] = "Y";
            drInfo["LOTID"]=  iRow<0? "" : dtInfo.Rows[iRow]["LOTID"];
            drInfo["PRDT_GRD_CODE"] = "";
            drInfo["FORM_WRK_TYPE_CODE"] = dt.Rows[0]["CBO_CODE"].ToString();  // [20180120-01]
            //drInfo["RCV_ISS_ID"] = dtInfo.Rows[iRow]["RCV_ISS_ID"];
            //drInfo["RN"] = dtInfo.Rows[iRow++]["RN"];
            dtInfo.Rows.InsertAt(drInfo, ++iRow);
            Util.GridSetData(dgLotInfo, dtInfo, FrameOperation, true);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtInfo = DataTableConverter.Convert(dgLotInfo.ItemsSource);

            List<DataRow> drInfo = dtInfo.Select("CHK = 'True'")?.ToList();
            foreach (DataRow dr in drInfo)
            {
                dtInfo.Rows.Remove(dr);
            }
            Util.GridSetData(dgLotInfo, dtInfo, FrameOperation, true);

            txtReturnQty.Value = Util.NVC_Int(dtInfo.Compute("SUM(LOTQTY2)", string.Empty));
            txtQty.Value = txtReqQty.Value - txtReturnQty.Value;

            //DataTable dtChk = dtInfo.Select("CHK is null OR CHK <> 'True'")?.CopyToDataTable();

            //if (dtChk.Rows.Count == dtInfo.Rows.Count)
            //{
            //    // SFU1645 선택된 작업대상이 없습니다.
            //    Util.MessageValidation("SFU1645");
            //    return;
            //}

            //Util.GridSetData(dgLotInfo, dtChk, FrameOperation, true);
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU4313", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetReturn();
                }
            });
        }

        /// <summary>
        /// 조회
        /// BIZ : BR_PRD_GET_PALLET_FOR_RETURN_FM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SetCbFormReturnTypeCode();
            GetPalletInfo();
            chkAllReturn.IsChecked = false;
            ClearLotInfoGrid();
            (dgLotInfo.Columns["FORM_WRK_TYPE_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(SetFormReturnTypeCode().Copy());
        }

        private void btn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (string.IsNullOrEmpty(txtShift.Text))
                //{
                //    // SFU1845 작업조를 입력하세요.
                //    Util.MessageValidation("SFU1845");
                //    return;
                //}

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    // SFU1843 작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                //int row = _util.GetDataGridCheckFirstRowIndex(dgHist, "CHK");

                //if (row < 0)
                //{
                //    // SFU1645 선택된 작업대상이 없습니다.
                //    Util.MessageValidation("SFU1645");
                //    return;
                //}

                List<int> rowList = _util.GetDataGridCheckRowIndex(dgHist, "CHK");

                if (rowList.Count <= 0)
                {
                    // SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                //string sRcvIssId = Util.NVC(dgReturn.GetCell(row, dgReturn.Columns["RCV_ISS_ID"].Index).Value);
                //string sBoxID = Util.NVC(dgReturn.GetCell(row, dgReturn.Columns["BOXID"].Index).Value);
                //string sRcvIssStatCode = Util.NVC(dgReturn.GetCell(row, dgReturn.Columns["RCV_ISS_STAT_CODE"].Index).Value);

                //if (sRcvIssStatCode != "FINISH_RECEIVE" || String.IsNullOrEmpty(sBoxID))
                //{
                //    Util.MessageValidation("1134", new string[] { sRcvIssId });
                //    return;
                //}

            //    string sProdID = Util.NVC(dgReturn.GetCell(row, dgReturn.Columns["PRODID"].Index).Value);

                foreach (int r in rowList)
                {
                    string sPalletID = Util.NVC(dgHist.GetCell(r, dgHist.Columns["PALLETID"].Index).Value);
                    string sProdID = Util.NVC(dgHist.GetCell(r, dgHist.Columns["PRODID"].Index).Value);
                  //  string sSeq = Util.NVC(dgHist.GetCell(r, dgHist.Columns["WIPSEQ"].Index).Value);
                    PrintTag(sProdID, sPalletID, "");
                }
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

        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtInfo = DataTableConverter.Convert(dgHist.ItemsSource);

            List<DataRow> drList = dtInfo.Select("CHK = 'True'").ToList();

            if (drList.Count <= 0)
            {
                // SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            loadingIndicator.Visibility = Visibility.Visible;
            object[] sParam = { drList.CopyToDataTable() };
                     
            this.FrameOperation.OpenMenu("SFU010120290", true, sParam);
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void btnTerm_Click(object sender, RoutedEventArgs e)
        {
            List<int> rowList = _util.GetDataGridCheckRowIndex(dgLotInfo, "CHK");

            if (rowList.Count <= 0)
            {
                // SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            if (rowList.Count > 1)
            {
                // SFU2004 하나의 Pallet만 선택해주십시오.
                Util.MessageValidation("SFU2004");
                return;
            }

            string sPalletID = Util.NVC(dgLotInfo.GetCell(rowList[0], dgLotInfo.Columns["BOXID"].Index).Value);

            loadingIndicator.Visibility = Visibility.Visible;
            string[] sParam = { sPalletID };
            this.FrameOperation.OpenMenu("SFU010120360", true, sParam);
            loadingIndicator.Visibility = Visibility.Collapsed;
        }


        #endregion

        #region [CheckBox]

        private void dgLotInfo_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre1.Content = chkAll1;
                            e.Column.HeaderPresenter.Content = pre1;
                            chkAll1.Checked -= new RoutedEventHandler(checkAll1_Checked);
                            chkAll1.Unchecked -= new RoutedEventHandler(checkAll1_Unchecked);
                            chkAll1.Checked += new RoutedEventHandler(checkAll1_Checked);
                            chkAll1.Unchecked += new RoutedEventHandler(checkAll1_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        void checkAll1_Checked(object sender, RoutedEventArgs e)
        {
            
            if ((bool)chkAll1.IsChecked)
            {            
                for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll1_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll1.IsChecked)
            {
                for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void dgHist_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre2.Content = chkAll2;
                            e.Column.HeaderPresenter.Content = pre2;
                            chkAll2.Checked -= new RoutedEventHandler(checkAll2_Checked);
                            chkAll2.Unchecked -= new RoutedEventHandler(checkAll2_Unchecked);
                            chkAll2.Checked += new RoutedEventHandler(checkAll2_Checked);
                            chkAll2.Unchecked += new RoutedEventHandler(checkAll2_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        void checkAll2_Checked(object sender, RoutedEventArgs e)
        {

            if ((bool)chkAll2.IsChecked)
            {
                for (int i = 0; i < dgHist.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                   // if (Util.NVC(DataTableConverter.GetValue(dgHist.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgHist.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgHist.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll2_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll2.IsChecked)
            {
                for (int i = 0; i < dgHist.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgHist.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        #endregion


        #endregion

        #region Mehod

        #region [BizCall]

        private void GetPalletInfo()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Util.NVC(cboArea.SelectedValue)) || Util.NVC(cboArea.SelectedValue) == "SELECT")
                {
                    // SFU1499 동을 선택해주세요.
                    Util.MessageValidation("SFU1499");
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("PKG_LOTID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_STAT_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                dr["RCV_ISS_STAT_CODE"] = string.IsNullOrWhiteSpace(Util.NVC(cboStat.SelectedValue)) ? null : cboStat.SelectedValue;

                if (!string.IsNullOrWhiteSpace(txtPalletID.Text))
                {
                    dr["BOXID"] = txtPalletID.Text;
                }
           
                else
                {
                    dr["AREAID"] = cboArea.SelectedValue;
                    dr["PKG_LOTID"] = string.IsNullOrWhiteSpace(txtLotID.Text) ? null : txtLotID.Text;
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PALLET_FOR_RETURN_FM", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                Util.GridSetData(dgReturn, dtResult, FrameOperation, true);
                txtPalletQty.Value = dgReturn.Rows.Count - dgReturn.BottomRows.Count;
                txtCellQty.Value = Util.NVC_Int(dtResult.Compute("SUM(ISS_QTY2)", string.Empty));

               // ClearLotInfoGrid();

                if (dgReturn.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgReturn.Columns["ISS_QTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }

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

        /// <summary>
        /// [20180120-01] 반품 유형 콤보 박스 세팅
        /// </summary>
        private void SetCbFormReturnTypeCode()
        {
            try
            {
                C1ComboBox cbo = cboFormReturnTypeCode;
                DataTable RQSTDT = new DataTable();

                DataTable dtResult = SetFormReturnTypeCode();

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = dtResult.Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// [20180120-01] 반품 유형 데이터 조회
        /// </summary>
        /// <returns></returns>
        private DataTable SetFormReturnTypeCode()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = cboArea.SelectedValue; ;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_RETURN_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            return dtResult;
        }

        /// <summary>
        /// 반품 Lot 상세 조회
        /// BIZ : BR_PRD_GET_LOT_FOR_RETURN_FM
        /// </summary>
        private void GetLotInfo(C1DataGrid datagrid, C1DataGrid datagrid2)
        {
            try
            {
                Util.gridClear(datagrid2);

                List<int> rowList = _util.GetDataGridCheckRowIndex(datagrid, "CHK");
                if (rowList.Count <= 0) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                int iReqQty = 0;

                //string sFirstProdid = Util.NVC(dgReturn.GetCell(rowList[0], dgReturn.Columns["PRODID"].Index).Value);
                //bool bChk = true;


                string sProdid = "";
                string sStat = "";

                foreach (int row in rowList)
                {
                    string sRcvIssID = Util.NVC(datagrid.GetCell(row, datagrid.Columns["RCV_ISS_ID"].Index).Value);
                    sProdid = Util.NVC(datagrid.GetCell(row, datagrid.Columns["PRODID"].Index).Value);
                    
                    iReqQty += Util.NVC_Int(datagrid.GetCell(row, datagrid.Columns["ISS_QTY2"].Index).Value);
                    sStat = Util.NVC(datagrid.GetCell(row, datagrid.Columns["RCV_ISS_STAT_CODE"].Index).Value);

                    //if (sFirstProdid != sProdid)
                    //{   
                    //    bChk = false;
                    //    //DataTableConverter.SetValue(dgReturn.Rows[row].DataItem, "CHK", false);
                    //    CheckBox box = dgReturn.GetCell(row, dgReturn.Columns["CHK"].Index).Presenter.Content as CheckBox;
                    //    box.IsChecked = false;
                    //}
                    //else
                    //{ 
                        DataRow dr = RQSTDT.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["RCV_ISS_ID"] = sRcvIssID;
                        RQSTDT.Rows.Add(dr);
                    //}
                }

                //if (!bChk)
                //{
                //    // SFU1502	동일 제품이 아닙니다.
                //    Util.MessageValidation("SFU1502");
                //}

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_LOT_FOR_RETURN_FM", "INDATA", "OUTDATA", RQSTDT);

                if (!dtResult.Columns.Contains("YN"))
                {
                    DataColumn dc = new DataColumn("YN");
                    dc.DefaultValue = "N";
                    dtResult.Columns.Add(dc);
                }


                if (datagrid.Name == "dgReturn")
                {
                    // C20180417_64069 반품 컬럼 추가로 제외 처리 함
                    //DataView dv = cboFormReturnTypeCode.ItemsSource as DataView;
                    //DataTable dt = dv.ToTable();

                    //DataColumn dc = new DataColumn("FORM_WRK_TYPE_CODE");
                    //dc.DefaultValue = dt.Rows[0]["CBO_CODE"].ToString();
                    //dtResult.Columns.Add(dc);

                    (dgLotInfo.Columns["SOC_VALUE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(setSOCCombo(sProdid));
                }

                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");

                //if (!dtResult.Columns.Contains("RN"))
                //    dtResult.Columns.Add("RN");

                //if (dtResult.Rows.Count > 0)
                //{
                //    dtResult = dtResult.AsEnumerable().OrderByDescending(c => c.Field<string>("RCV_ISS_ID")).OrderBy(c => c.Field<string>("LOTID")).CopyToDataTable();

                //    string sIssID = (string)dtResult.Rows[0]["RCV_ISS_ID"];
                //    int iRn = 0;
                //    for (int cnt = 0; cnt < dtResult.Rows.Count; cnt++)
                //    {
                //        if ((string)dtResult.Rows[cnt]["RCV_ISS_ID"] == sIssID)
                //        {
                //            dtResult.Rows[cnt]["RN"] = iRn % 2;
                //        }
                //        else
                //        {
                //            sIssID = (string)dtResult.Rows[cnt]["RCV_ISS_ID"];
                //            dtResult.Rows[cnt]["RN"] = ++iRn % 2;
                //        }
                //    }
                //}

                Util.GridSetData(datagrid2, dtResult, FrameOperation, true);

                txtReqQty.Value = iReqQty;
                txtReturnQty.Value = Util.NVC_Int(dtResult.Compute("SUM(LOTQTY2)", string.Empty));
                txtQty.Value = txtReqQty.Value - txtReturnQty.Value;


                if (dgLotInfo.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(datagrid2.Columns["LOTQTY2"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }

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

        /// <summary>
        /// 반품확정
        /// BIZ : BR_PRD_REG_RETURN_PALLET_FM
        /// </summary>
        /// 
        private void SetReturn()
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    // SFU1843 작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                List<int> iReturnList = _util.GetDataGridCheckRowIndex(dgReturn, "CHK");

                if (iReturnList.Count <= 0)
                {
                    // SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                DataTable dtInfo = DataTableConverter.Convert(dgLotInfo.ItemsSource);

                if (dtInfo?.Rows?.Count <= 0)
                {
                    // SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (dtInfo.Select("LOTQTY2  IS NULL OR LOTQTY2 <= 0").Count() > 0)
                {
                    // SFU1684	수량을 입력하세요.
                    Util.MessageValidation("SFU1684");
                    return;
                }

                if (dtInfo.Select("SOC_VALUE  IS NULL OR trim(SOC_VALUE) = ''").Count() > 0)
                {
                    // SFU4072 SOC가 선택되지 않았습니다.
                    Util.MessageValidation("SFU4072");
                    return;
                }

                if (dtInfo.Select("CAPA_GRD_CODE  IS NULL OR trim(CAPA_GRD_CODE) = ''").Count() > 0)
                {
                    // SFU4072 용량등급을 선택해 주세요
                    Util.MessageValidation("SFU4022");
                    return;
                }


                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                DataRow newRow = inDataTable.NewRow();
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("USERNAME");

                newRow = inDataTable.NewRow();
                newRow["USERID"] = txtWorker.Tag;
                newRow["USERNAME"] = txtWorker.Text;
                inDataTable.Rows.Add(newRow);


                newRow = null;

                DataTable inReturnTable = indataSet.Tables.Add("INRETURN");             
                inReturnTable.Columns.Add("RCV_ISS_ID");

                int iReqQty = 0;
             

                foreach (int row in iReturnList)
                {
                    //string sType = Util.NVC(dgReturn.GetCell(row, dgReturn.Columns["RCV_ISS_STAT_CODE"].Index).Value);
                    //if (sType != "SHIPPING")
                    //{
                    //    return;
                    //}
                    newRow = inReturnTable.NewRow();
                    newRow["RCV_ISS_ID"] = Util.NVC(dgReturn.GetCell(row, dgReturn.Columns["RCV_ISS_ID"].Index).Value);
                    iReqQty += Util.NVC_Int(dgReturn.GetCell(row, dgReturn.Columns["ISS_QTY2"].Index).Value);
                    inReturnTable.Rows.Add(newRow);
                }
                int iRetQty = dtInfo.AsEnumerable().Sum(c => Util.NVC_Int(c.Field<int>("LOTQTY2")));
                if (iReqQty != iRetQty)
                {
                    // SFU3134	반품수량과 Cell 수량이 일치하지 않습니다.
                    Util.MessageValidation("SFU3134");
                    return;
                }

                newRow = null;

                DataTable inLotTable = indataSet.Tables.Add("INLOT");
                inLotTable.Columns.Add("LOTID");
                inLotTable.Columns.Add("PRDT_GRD_CODE");
                inLotTable.Columns.Add("LOTQTY");
                inLotTable.Columns.Add("FORM_WRK_TYPE_CODE");
                inLotTable.Columns.Add("WIP_NOTE");
                //C20180417_64069 추가
                inLotTable.Columns.Add("SOC_VALUE");
                inLotTable.Columns.Add("CAPA_GRD_CODE");

                

                foreach (DataRow dr in dtInfo.Rows)
                {
                    newRow = inLotTable.NewRow();
                    newRow["LOTID"] = dr["LOTID"].ToString().Trim();
                    newRow["PRDT_GRD_CODE"] = dr["PRDT_GRD_CODE"].ToString().Trim();
                    newRow["LOTQTY"] = dr["LOTQTY2"];
                    newRow["FORM_WRK_TYPE_CODE"] = dr["FORM_WRK_TYPE_CODE"].ToString().Trim();

                    newRow["SOC_VALUE"] = dr["SOC_VALUE"].ToString().Trim();
                    newRow["CAPA_GRD_CODE"] = dr["CAPA_GRD_CODE"].ToString().Trim();

                    newRow["WIP_NOTE"] = dr["WIP_NOTE"];
                    inLotTable.Rows.Add(newRow);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                //new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RETURN_PALLET_LIST_FM", "INDATA,INRETURN,INLOT", "OUTBOX,OUTPALLET", (bizResult, bizException) =>
                //{
                //    try
                //    {
                //        if (bizException != null)
                //        {
                //            Util.MessageException(bizException);
                //            return;
                //        }
                //        // SFU1275 정상 처리 되었습니다.
                //        Util.MessageInfo("SFU1275");

                //        chkAllReturn.IsChecked = false;
                //        GetPalletInfo();
                //        //  _util.SetDataGridCheck(dgReturn, "CHK", "RCV_ISS_ID", sRcvIssID);
                //      //  DataTableConverter.SetValue(dgReturn.Rows[row].DataItem, "CHK", true);

                //      //  GetLotInfo();

                //        //for (int r= 0; r< dgLotInfo?.Rows?.Count - dgLotInfo?.Rows?.BottomRows?.Count; r++)
                //        //{
                //        //    string sPalletID = Util.NVC(dgLotInfo.GetCell(r, dgLotInfo.Columns["BOXID"].Index).Value);
                //        //    string sSeq = Util.NVC(dgLotInfo.GetCell(r, dgLotInfo.Columns["WIPSEQ"].Index).Value);

                //        //    PrintTag(sProdId, sPalletID, sSeq);
                //        //}
                //    }
                //    catch (Exception ex)
                //    {
                //        Util.MessageException(ex);
                //    }
                //    finally
                //    {

                //    }
                //}, indataSet);

                //DataTable RQSTDT = new DataTable();
                //RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                //dr["RCV_ISS_ID"] = sRcvIssID;
                //RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_LOT_FOR_RETURN_FM", "INDATA", "OUTDATA", RQSTDT);
                //if (!dtResult.Columns.Contains("CHK"))
                //    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                //Util.GridSetData(dgLotInfo, dtResult, FrameOperation, true);

                //if (dgLotInfo.Rows.Count > 0)
                //{
                //    DataGridAggregate.SetAggregateFunctions(dgLotInfo.Columns["LOTQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                //}

                DataSet dsResult = new DataSet();
                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RETURN_PALLET_LIST_FM", "INDATA,INRETURN,INLOT", "OUTBOX,OUTPALLET", indataSet, null);

                Util.MessageInfo("SFU1275");

                chkAllReturn.IsChecked = false;
                GetPalletInfo();

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

        /// <summary>
        /// 전압등급,레벨 콤보 셋팅
        /// BIZ : DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE
        /// </summary>
        /// <param name="sFilter"></param>
        /// <returns></returns>
        private DataTable dtTypeCombo(string sFilter)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = sFilter;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);
            AddStatus(dtResult, CommonCombo.ComboStatus.NA, "CBO_CODE", "CBO_NAME");
            return dtResult;
        }

        /// <summary>
        /// C20180417_64069 
        /// capa 등 공통 모듈 조회
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="comTypeCode"></param>
        /// <param name="attr1"></param>
        private DataTable dtCapaCombo(string comTypeCode, string attr1 = null)
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
                newRow["COM_TYPE_CODE"] = comTypeCode;
                newRow["ATTR1"] = attr1;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_GRADE_TYPE_CODE_CBO", "INDATA", "OUTDATA", inTable);

                //DataRow dr = dtResult.NewRow();
                //dr["CBO_CODE"] = "";
                // dr["CBO_NAME"] = " - SELECT-";
                // dtResult.Rows.InsertAt(dr, 0);

                return dtResult;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }

        }

        /// <summary>
        /// C20180417_64069
        /// SOC 조회 기능
        /// </summary>
        /// <param name="cbo"></param>
        /// <param name="cs"></param>
        /// <param name="prodID"></param>
        private DataTable setSOCCombo(string prodID)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["PRODID"] = prodID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRDT_SOC_VLTG_COMBO", "INDATA", "OUTDATA", inTable);

                //DataRow dr = dtResult.NewRow();
                //dr["CBO_CODE"] = "";
                // dr["CBO_NAME"] = " - SELECT-";
                // dtResult.Rows.InsertAt(dr, 0);

                return dtResult;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        #endregion

        #region [Validation]

        #endregion

        #region [PopUp Event]
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER wndPopup = new GMES.MES.CMM001.Popup.CMM_SHIFT_USER();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = "";
                Parameters[1] = cboArea.SelectedValue;
                Parameters[2] = "";
                Parameters[3] = Process.GRADING;
                Parameters[4] = "";
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShift_Closed);
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        private void wndShift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER wndPopup = sender as CMM_SHIFT_USER;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
            }
            this.grdMain.Children.Remove(wndPopup);
        }

        #endregion

        #region [Func]

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ClearLotInfoGrid()
        {
            Util.gridClear(dgLotInfo);
            txtReqQty.Value = txtReturnQty.Value = txtQty.Value = 0;
        }



        #endregion

        #endregion



        private void PrintTag(string sProdID, string sBoxID, string seq)
        {
            CMM_FORM_RETURN_TAG_PRINT popupTagPrint = new CMM_FORM_RETURN_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;

            object[] parameters = new object[8];
            parameters[0] = Process.SmallPacking;// strProdID == "MCC"? Process.CircularCharacteristic : Process.SmallPacking;
            parameters[1] = sProdID; //prodid  // Util.NVC(ComboEquipment.SelectedValue);
            parameters[2] = sBoxID; //DataTableConverter.GetValue(DgProductionPallet.Rows[iRow].DataItem, "PALLETE_ID").GetString();
            parameters[3] = seq; //DataTableConverter.GetValue(DgProductionPallet.Rows[iRow].DataItem, "WIPSEQ").GetString();
            parameters[4] = ""; //DataTableConverter.GetValue(DgProductionPallet.Rows[iRow].DataItem, "CELL_QTY").GetString();
            //parameters[5] = DataTableConverter.GetValue(DgProductionPallet.Rows[iRow].DataItem, "DISPATCH_YN").GetString();
            parameters[5] = ""; //DataTableConverter.GetValue(DgProductionPallet.Rows[iRow].DataItem, "WIPHOLD").GetString().Equals("Y") ? "N" : "Y";      // 디스패치 처리
            parameters[6] = ""; //DataTableConverter.GetValue(DgProductionPallet.Rows[iRow].DataItem, "PRINT_YN").GetString();
            parameters[7] = (bool)chkTagPrint.IsChecked ? "N" : "Y";     // Direct 출력 여부

            C1WindowExtension.SetParameters(popupTagPrint, parameters);

            popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);
            grdMain.Children.Add(popupTagPrint);
            popupTagPrint.BringToFront();
        }

        private void popupTagPrint_Closed(object sender, EventArgs e)
        {
            CMM_FORM_RETURN_TAG_PRINT popup = sender as CMM_FORM_RETURN_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(popup);
        }
        
        private void dgReturn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           
            //dtInfo = dtInfo.Select("CHK = 'True'")?.CopyToDataTable();

            //int cnt = dtInfo.DefaultView.ToTable(true, "PRODID").Rows.Count;
          
            //if (!bChk)
            //{
            //    // SFU1502	동일 제품이 아닙니다.
            //    Util.MessageValidation("SFU1502");
            //}
        }
        private void dgReturn_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            C1DataGrid datagrid = sender as C1DataGrid;

            if (datagrid.CurrentColumn.Name == "CHK")
            {
                DataTable dtInfo = DataTableConverter.Convert(datagrid.ItemsSource);
                if (dtInfo.Select("CHK = 'True'").ToList().Count > 0)
                {
                    dtInfo = dtInfo.Select("CHK = 'True'").CopyToDataTable();
                    int cnt = dtInfo.DefaultView.ToTable(true, "PRODID").Rows.Count;

                    if (cnt > 1)
                    {
                        // SFU1502	동일 제품이 아닙니다.
                        Util.MessageValidation("SFU1502");

                        if ((bool)DataTableConverter.GetValue(e.Row.DataItem, "CHK"))
                        {
                            DataTableConverter.SetValue(datagrid.Rows[e.Row.Index].DataItem, "CHK", false);
                           // e.Cancel = true;                           
                        }
                    }
                }
            }
        }
        
        private void dgReturn_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid datagrid = sender as C1DataGrid;

            try
            {
                if (datagrid.CurrentRow == null)
                    return;

                if (datagrid.CurrentColumn.Name == "CHK")
                {
                    GetLotInfo(datagrid, datagrid.Name == "dgReturn"? dgLotInfo : dgLotInfo3);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }        

        private void dgLotInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    //if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RN")).Equals("0"))
                    //{
                    //    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("White"));
                    //}
                    //else
                    //    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF3F0F0"));
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnAdjust_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLotId_Input.Text))
            {
                // 
                Util.MessageValidation("LOTID를 확인하세요.");
                return;
            }

            DataTable dtVolt = DataTableConverter.Convert((dgLotInfo.Columns["PRDT_GRD_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource);

            int cnt = dtVolt.Select("CBO_CODE = '" + txtPrdtGrd_Input.Text.Trim() + "'").ToList().Count;

            if (cnt <= 0)
            {
                Util.MessageValidation("전압등급을 확인하세요.");
                return;
            }

            //C20180417_64069 
            DataTable dtCapa = DataTableConverter.Convert((dgLotInfo.Columns["CAPA_GRD_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource);

            cnt = dtCapa.Select("CBO_CODE = '" + txtCapaGrd_Input.Text.Trim() + "'").ToList().Count;

            if (cnt <= 0)
            {
                Util.MessageValidation("용량등급을 확인하세요.");
                return;
            }

            //C20180417_64069 
            DataTable dtSoc = DataTableConverter.Convert((dgLotInfo.Columns["SOC_VALUE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource);

            cnt = dtSoc.Select("CBO_NAME = '" + txtSOC_Input.Text.Trim() + "'").ToList().Count;

            if (cnt <= 0)
            {
                Util.MessageValidation("SOC 값을 확인하세요.");
                return;
            }


            List<int> iList = _util.GetDataGridCheckRowIndex(dgLotInfo, "CHK");

            if (iList.Count <= 0)
            {
                // SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            DataTable dtInfo = DataTableConverter.Convert(dgLotInfo.ItemsSource);
            foreach (int i in iList)
            {
                dtInfo.Rows[i]["LOTID"] = txtLotId_Input.Text.Trim();
                dtInfo.Rows[i]["PRDT_GRD_CODE"] = txtPrdtGrd_Input.Text.Trim();
                dtInfo.Rows[i]["FORM_WRK_TYPE_CODE"] = cboFormReturnTypeCode.SelectedValue;
                dtInfo.Rows[i]["CAPA_GRD_CODE"] = txtCapaGrd_Input.Text.Trim();
                dtInfo.Rows[i]["SOC_VALUE"] = txtSOC_Input.Text.Trim();
            }
            Util.GridSetData(dgLotInfo, dtInfo, FrameOperation, true);
        }

        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtInfo = DataTableConverter.Convert(dgLotInfo.ItemsSource);
            
            var list = dtInfo.AsEnumerable().GroupBy(g => new { LOTID = g.Field<string>("LOTID")
                                                    , PRDT_GRD_CODE = g.Field<string>("PRDT_GRD_CODE")
            }
            ).Select(c=> new { c.Key.LOTID, c.Key.PRDT_GRD_CODE, LOTQTY = c.Sum(s=>Util.NVC_Int(s.Field<object>("LOTQTY2")))}).ToList();

            DataTable dtNew = new DataTable();
            dtNew.Columns.Add("CHK");
            dtNew.Columns.Add("LOTID");
            dtNew.Columns.Add("PRDT_GRD_CODE"); 
            dtNew.Columns.Add("LOTQTY2",typeof(int));
            dtNew.Columns.Add("WIP_NOTE");

            foreach (var item in list)
            {
                DataRow drNew = dtNew.NewRow();
                drNew["LOTID"] = item.LOTID;
                drNew["PRDT_GRD_CODE"] = item.PRDT_GRD_CODE;
                drNew["LOTQTY2"] = item.LOTQTY;
                dtNew.Rows.Add(drNew);
            }

            Util.GridSetData(dgLotInfo, dtNew, FrameOperation, true);
        }

        private void btnSearch_Hist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("PKG_LOTID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrWhiteSpace(txtPalletID2.Text))
                {
                    dr["BOXID"] = txtPalletID2.Text;
                }
                else
                {
                    dr["AREAID"] = string.IsNullOrWhiteSpace(Util.NVC(cboArea2.SelectedValue))? null : cboArea2.SelectedValue;
                    dr["FROM_DTTM"] = dtpDateFrom2.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                    dr["TO_DTTM"] = dtpDateTo2.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                    dr["PKG_LOTID"] = string.IsNullOrWhiteSpace(txtLotID2.Text)? null : txtLotID2.Text;
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_RETURN_PALLET_LIST_FM", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult.Columns.Add("CHK");
                Util.GridSetData(dgHist, dtResult, FrameOperation, true);   

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

        private void btnReturnReg3_Click(object sender, RoutedEventArgs e)
        {
            BOX001_102_RETURN popup = new BOX001_102_RETURN();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[7];
                Parameters[0] = "";
                Parameters[1] = "";
                Parameters[2] = "";
                Parameters[3] = "";
                Parameters[4] = "";
                Parameters[5] = txtWorker.Tag;
                Parameters[6] = txtWorker.Text;

                C1WindowExtension.SetParameters(popup, Parameters);

                //  popup.Closed += new EventHandler(puInboxLabel_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }

        }

        private void btnReturnCan3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnReturn3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSearch3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("PKG_LOTID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrWhiteSpace(txtPalletID3.Text))
                {
                    dr["BOXID"] = txtPalletID3.Text;
                }
                else
                {
                    dr["AREAID"] = string.IsNullOrWhiteSpace(Util.NVC(cboArea3.SelectedValue)) ? null : cboArea3.SelectedValue;
                    dr["FROM_DTTM"] = dtpDateFrom3.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                    dr["TO_DTTM"] = dtpDateTo3.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                    dr["PKG_LOTID"] = string.IsNullOrWhiteSpace(txtLotID3.Text) ? null : txtLotID3.Text;
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PALLET_FOR_RETURN_SUBCONTRACT_FM", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult.Columns.Add("CHK");
                Util.GridSetData(dgReturn3, dtResult, FrameOperation, true);

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

        private void dgReturn_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            preReturn.Content = chkAllReturn;
                            e.Column.HeaderPresenter.Content = preReturn;
                            chkAllReturn.Checked -= new RoutedEventHandler(chkAllReturn_Checked);
                            chkAllReturn.Unchecked -= new RoutedEventHandler(chkAllReturn_Unchecked);
                            chkAllReturn.Checked += new RoutedEventHandler(chkAllReturn_Checked);
                            chkAllReturn.Unchecked += new RoutedEventHandler(chkAllReturn_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        void chkAllReturn_Checked(object sender, RoutedEventArgs e)
        {

            if ((bool)chkAllReturn.IsChecked)
            {
                DataTable dtInfo = DataTableConverter.Convert(dgReturn.ItemsSource);

                int cntProd = dtInfo.DefaultView.ToTable(true, "PRODID").Rows.Count;
                if (cntProd > 1)
                {
                    // 동일 제품이 아닙니다.
                    Util.MessageValidation("SFU1502");
                    return;
                }

                if (dtInfo.Select("RCV_ISS_STAT_CODE <> 'SHIPPING'").Count() > 0)
                {
                    // 반품상태가 반품 요청이 아닌 반품ID 가 존재합니다.
                    Util.MessageValidation("SFU4941");
                    return;
                }

                for (int i = 0; i < dgReturn.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgReturn.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                    {
                        DataTableConverter.SetValue(dgReturn.Rows[i].DataItem, "CHK", true);
                    }
                }

                GetLotInfo(dgReturn, dgLotInfo);
            }
        }
        private void chkAllReturn_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAllReturn.IsChecked)
            {
                for (int i = 0; i < dgReturn.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgReturn.Rows[i].DataItem, "CHK", false);
                }

                ClearLotInfoGrid();
            }
        }
    }
}
