/*************************************************************************************
 Created Date : 2017.11.20
      Creator : 이슬아
   Decription : 전지 5MEGA-GMES 구축 - 출하HOLD 관리
--------------------------------------------------------------------------------------
 [Change History]


**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_218 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        public BOX001_218()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_AREA_ID, "B" }, sCase: "PROCESSSEGMENTLINE");
            _combo.SetCombo(cboProcType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "B" }, sCase: "PROCBYPCSGID");

            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboAreaHistory, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboAreaHistory };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, sCase: "PROCESSSEGMENTLINE", cbParent: cboEquipmentSegmentParent, sFilter: new string[] {"B" });

            //공정
            _combo.SetCombo(cboProcessHistory, CommonCombo.ComboStatus.ALL, sFilter: new string[] { "B" }, sCase: "PROCBYPCSGID");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Today;
            dtpDateTo.SelectedDateTime = DateTime.Today;         
        }        
        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }
        #endregion

        #region [대차 병합]

        #region  체크박스 선택 이벤트     
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgContainer.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgContainer.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgContainer.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgContainer.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        #endregion

        #region [이벤트]
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440  초기화 하시겠습니까? 
            Util.MessageConfirm("SFU3440", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Clear();
                }
            });
        }

        private void txtCtnrID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    //SFU1223 라인을 선택 하세요
                    Util.MessageValidation("SFU1223");
                    return;
                }

                else if (Util.NVC(cboProcType.SelectedValue) == "SELECT")
                {
                    // SFU4343     포장 구분을 선택하세요.
                    Util.MessageValidation("SFU4343");
                    return;
                }

                else if (string.IsNullOrWhiteSpace(txtCtnrID.Text))
                {
                    //	SFU2860	대차ID를 먼저 스캔하여 주시기 바랍니다.
                    Util.MessageValidation("SFU2860", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCtnrID.Focus();
                            txtCtnrID.Text = string.Empty;
                        }
                    });
                    return;
                }

                if (dgContainer.GetRowCount() > 0)
                {
                    DataTable dtInfo = DataTableConverter.Convert(dgContainer.ItemsSource);
                    DataRow[] drList = dtInfo.Select("CTNR_ID = '" + txtCtnrID.Text.Trim() + "'");

                    if (drList.Length > 0)
                    {
                        dgContainer.SelectedIndex = dtInfo.Rows.IndexOf(drList[0]);
                        txtCtnrID.Focus();
                        txtCtnrID.Text = string.Empty;
                        return;
                    }
                }

                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("EQSGID");
                inDataTable.Columns.Add("PROCID");
                inDataTable.Columns.Add("CTNR_ID");

                DataRow newRow = inDataTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = cboLine.SelectedValue;
                newRow["PROCID"] = cboProcType.SelectedValue;
                newRow["CTNR_ID"] = txtCtnrID.Text.Trim();
                inDataTable.Rows.Add(newRow);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_PRD_SEL_CART_INFO_NJ", "INDATA", "OUDATA", inDataTable, (dtRslt, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (dtRslt.Rows.Count <= 0)
                        {
                            //SFU4365		대차 정보가 없습니다.	
                            Util.MessageInfo("SFU4365");
                            return;
                        }

                        DataTable dtInfo = DataTableConverter.Convert(dgContainer.ItemsSource);
                        dtRslt.Merge(dtInfo);

                        Util.GridSetData(dgContainer, dtRslt, FrameOperation, true);

                        if (dgContainer.Rows.Count > 0)
                        {
                            SetDetailInfo();

                            DataGridAggregate.SetAggregateFunctions(dgContainer.Columns["CTNR_ID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                            DataGridAggregate.SetAggregateFunctions(dgContainer.Columns["INBOX_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            DataGridAggregate.SetAggregateFunctions(dgContainer.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        }

                        txtCtnrID.Focus();
                        txtCtnrID.Text = string.Empty;
                        SetReadOnly(true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<int> idxList = _util.GetDataGridCheckRowIndex(dgContainer, "CHK");

                if (idxList.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                //SFU4174		병합하시겠습니까?	
                Util.MessageConfirm("SFU4174", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        DataSet indataSet = new DataSet();
                        DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("SRCTYPE");
                        inDataTable.Columns.Add("USERID");
                        inDataTable.Columns.Add("PROCID");
                        inDataTable.Columns.Add("AREAID");

                        DataRow newRow = inDataTable.NewRow();
                        newRow["SRCTYPE"] = "UI";
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["PROCID"] = cboProcType.SelectedValue;
                        newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                        inDataTable.Rows.Add(newRow);

                        DataTable inCtnrTable = indataSet.Tables.Add("INCTNR");
                        inCtnrTable.Columns.Add("CTNR_ID");

                        foreach (int idx in idxList)
                        {
                            string sCtnrID = Util.NVC(dgContainer.GetCell(idx, dgContainer.Columns["CTNR_ID"].Index).Value);

                            newRow = inCtnrTable.NewRow();
                            newRow["CTNR_ID"] = sCtnrID;
                            inCtnrTable.Rows.Add(newRow);
                        }
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_COMPOSE_CART_NJ", "INDATA,INCTNR", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                if (bizResult.Tables.Contains("OUTDATA"))
                                {
                                    string sCtnrID = Util.NVC(bizResult.Tables["OUTDATA"].Rows[0]["TO_CTNR_ID"]);
                                    POLYMER_TagPrint(sCtnrID);
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

                        }, indataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void dgContainer_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;
                            e.Column.HeaderPresenter.Content = pre;
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
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

        private void dgContainer_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Name == "CHK")
            {
                if (_util.GetDataGridCheckCnt(dgContainer, "CHK") > 0)
                    SetDetailInfo();
                else
                    Util.gridClear(dgLot);
            }
        }
        #endregion

        #region Method
        private void SetReadOnly(bool bReadOnly)
        {
            cboLine.IsEnabled = !bReadOnly;
            cboProcType.IsEnabled = !bReadOnly;
        }
        private void Clear()
        {
            chkAll.IsChecked = false;
            Util.gridClear(dgContainer);
            Util.gridClear(dgLot);
            SetReadOnly(false);
        }

        private void POLYMER_TagPrint(string sCtnrID)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupTagPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;
            popupTagPrint.CART_MERGE = "Y";
            popupTagPrint.PrintCount = "1";
            
            object[] parameters = new object[5];
            parameters[0] = cboProcType.SelectedValue;       // _processCode;
            parameters[1] = string.Empty;     // Util.NVC(ComboEquipment.SelectedValue);
            parameters[2] = sCtnrID;
            parameters[3] = "N";     // Direct 출력 여부
            parameters[4] = "N";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupTagPrint, parameters);

            popupTagPrint.Closed += new EventHandler(POLYMER_popupTagPrint_Closed);
            grdMain.Children.Add(popupTagPrint);
            popupTagPrint.BringToFront();
        }
        private void POLYMER_popupTagPrint_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_TAG_PRINT popup = sender as CMM_POLYMER_FORM_TAG_PRINT;
            if (popup != null)
            {
                Clear();
            }
            this.grdMain.Children.Remove(popup);
        }
        private void SetDetailInfo()
        {
            List<int> idxList = _util.GetDataGridCheckRowIndex(dgContainer, "CHK");

            if (idxList.Count <= 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }
            
            DataTable inDataTable = new DataTable("INDATA");
            inDataTable.Columns.Add("CTNR_ID");

            foreach (int idx in idxList)
            {
                string sCtnrID = Util.NVC(dgContainer.GetCell(idx, dgContainer.Columns["CTNR_ID"].Index).Value);

                DataRow newRow = inDataTable.NewRow();
                newRow["CTNR_ID"] = sCtnrID;
                inDataTable.Rows.Add(newRow);
            }

            loadingIndicator.Visibility = Visibility.Visible;

            new ClientProxy().ExecuteService("DA_PRD_SEL_CART_LOTID_RT_INFO_NJ", "INDATA", "OUDATA", inDataTable, (dtRslt, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgLot, dtRslt, FrameOperation, true);

                    if (dgLot.Rows.Count > 0)
                    {
                        DataGridAggregate.SetAggregateFunctions(dgLot.Columns["LOTID_RT"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                        DataGridAggregate.SetAggregateFunctions(dgLot.Columns["INBOX_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        DataGridAggregate.SetAggregateFunctions(dgLot.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
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
            });
        }
        #endregion

        #endregion

        #region [대차 병합 이력 조회]

        #region 기간 이벤트
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }
        #endregion

        #region 버튼 이벤트
        private void btnSearchHist_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtRqst = new DataTable("RQSTDT");
            dtRqst.Columns.Add("FROM_DATE", typeof(String));
            dtRqst.Columns.Add("TO_DATE", typeof(String));
            dtRqst.Columns.Add("EQSGID", typeof(string));
            dtRqst.Columns.Add("PROCID", typeof(string));
            dtRqst.Columns.Add("PJT_NAME", typeof(string));
            dtRqst.Columns.Add("PRODID", typeof(string));
            dtRqst.Columns.Add("LOTID_RT", typeof(string));
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("CTNR_ID", typeof(string));

            //ldpDateFromHist.SelectedDateTime.ToShortDateString();
            DataRow dr = dtRqst.NewRow();
            dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
            dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
            dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
            dr["PROCID"] = Util.GetCondition(cboProcessHistory, bAllNull: true);
            dr["PJT_NAME"] = string.IsNullOrWhiteSpace(txtPjtHistory_Detail.Text) ? null : txtPjtHistory_Detail.Text;
            dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdID_Detail.Text)? null : txtProdID_Detail.Text;
            dr["LOTID_RT"] = string.IsNullOrWhiteSpace(txtLotRTHistory_Detail.Text) ? null : txtLotRTHistory_Detail.Text;
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CTNR_ID"] = Util.GetCondition(txtCTNR_IDHistory);
            dtRqst.Rows.Add(dr);

            loadingIndicator.Visibility = Visibility.Visible;

            new ClientProxy().ExecuteService("DA_PRD_SEL_POLYMER_CART_MERGE_HISTORY", "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }                 

                    Util.GridSetData(dgCartHistory, bizResult, FrameOperation, true);

                    if (dgCartHistory.Rows.Count > 0)
                    {
                        DataGridAggregate.SetAggregateFunctions(dgCartHistory.Columns["PRE_WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        DataGridAggregate.SetAggregateFunctions(dgCartHistory.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        DataGridAggregate.SetAggregateFunctions(dgCartHistory.Columns["TO_CTNR_PRE_WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        DataGridAggregate.SetAggregateFunctions(dgCartHistory.Columns["TO_CTNR_WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
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
            });
        }

        private void btnRePrint_Click(object sender, RoutedEventArgs e)
        {
            List<int> idxList = _util.GetDataGridCheckRowIndex(dgCartHistory, "CHK");

            if (idxList.Count <= 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            foreach (int idx in idxList)
            {
                string sCtnrID = Util.NVC(dgCartHistory.GetCell(idx, dgCartHistory.Columns["TO_CTNR_ID"].Index).Value);

                POLYMER_TagPrint(sCtnrID);
            }
        }
        #endregion

        #region 이벤트
        #endregion

        #endregion







     
    }
}
