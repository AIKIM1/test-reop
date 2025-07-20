/*************************************************************************************
 Created Date : 2017.07.04
      Creator : 이슬아
   Decription : 포장 공정실적
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.04  이슬아 : Initial Created.
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
    public partial class BOX001_209 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private int _tagPrintCount;
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

        public BOX001_209()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: new C1ComboBox[] { cboLine });
            _combo.SetCombo(cboArea2, CommonCombo.ComboStatus.ALL, sCase: "cboArea");
            _combo.SetCombo(cboArea3, CommonCombo.ComboStatus.ALL, sCase: "cboArea");
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, cbParent: new C1ComboBox[] { cboArea }, sFilter: new string[] { "B" }, sCase: "PROCESSSEGMENTLINE");
            _combo.SetCombo(cboStat, CommonCombo.ComboStatus.ALL, sFilter: new string[] { "RETURN_RCV_ISS_STAT_CODE" }, sCase: "COMMCODE_WITHOUT_CODE");

            //(dgLotInfo.Columns["PRDT_GRD_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtTypeCombo("VLTG_GRD_CODE"));

            // 반품유형 콤보
            SetReturnTypeCombo();

            setShipToPopControl();
        }

        private void setShipToPopControl()
        {
            const string bizRuleName = "DA_PRD_SEL_SHIPTO_CBO_NJ";
            string[] arrColumn = { "SHOPID", "LANGID" };
            string[] arrCondition = { LoginInfo.CFG_SHOP_ID, LoginInfo.LANGID };

            CommonCombo.SetFindPopupCombo(bizRuleName, popShipto, arrColumn, arrCondition, (string)popShipto.SelectedValuePath, (string)popShipto.DisplayMemberPath);
            CommonCombo.SetFindPopupCombo(bizRuleName, popShiptoHist, arrColumn, arrCondition, (string)popShiptoHist.SelectedValuePath, (string)popShiptoHist.DisplayMemberPath);
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            SetCommCode(dgLotInfo, "CAPA_GRD_CODE", "G");
            SetCommCode(dgLotInfo, "RSST_GRD_CODE");
            SetCommCode(dgLotInfo, "VLTG_GRD_CODE");
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

            if (DataTableConverter.GetValue(e.Row.DataItem, "BOXID") != null)
            {
                if (e.Column.Name != "WIP_NOTE")
                {
                    e.Cancel = true;
                    return;
                }
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
            DataRow drInfo = dtInfo.NewRow();
            //drInfo["YN"] = "Y";
            drInfo["LOTID"] = iRow < 0 ? "" : dtInfo.Rows[iRow]["LOTID"];
            drInfo["PRDT_GRD_CODE"] = "";
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

            DataTable dtInfo = DataTableConverter.Convert(dgReturn.ItemsSource);
            DataTable dtLotInfo = DataTableConverter.Convert(dgLotInfo.ItemsSource);

            if (dtLotInfo?.Rows?.Count <= 0)
            {
                // SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            if (dtLotInfo.Select("LOTQTY2  IS NULL OR LOTQTY2 <= 0").Count() > 0)
            {
                // SFU1684	수량을 입력하세요.
                Util.MessageValidation("SFU1684");
                return;
            }

            if (dtLotInfo.AsEnumerable().Where(c=> string.IsNullOrWhiteSpace(c.Field<string>("LOTID"))).ToList().Count > 0)
            {
                // SFU4148	조립LOTID는 8자리 이상입니다.
                Util.MessageValidation("SFU4148");
                return;
            }

            //if (dtLotInfo.AsEnumerable().Where(c => c.Field<string>("LOTID").Length < 8).ToList().Count > 0)
            //{
            //    // SFU4148	조립LOTID는 8자리 이상입니다.
            //    Util.MessageValidation("SFU4148");
            //    return;
            //}


            int iRetQty = dtLotInfo.AsEnumerable().Sum(c => Util.NVC_Int(c.Field<int>("LOTQTY2")));
            int iReqQty = 0;

            foreach (int row in iReturnList)
            {
                iReqQty += Util.NVC_Int(dgReturn.GetCell(row, dgReturn.Columns["ISS_QTY2"].Index).Value);
            }

            if (iReqQty != iRetQty)
            {
                // SFU3134	반품수량과 Cell 수량이 일치하지 않습니다.
                Util.MessageValidation("SFU3134");
                return;
            }

            //SFU4437 반품 확정 하시겠습니까?
            Util.MessageConfirm("SFU4437", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataRow dr = dtInfo.Select("CHK = 'True' AND BOX_RESTORE_YN = 'Y'").FirstOrDefault();
                    if (dr != null)
                    {
                        //	SFU4456	출하 전 포장정보가 존재합니다. 포장 정보를 복원하시겠습니까? 
                         Util.MessageConfirm("SFU4456", (result2) =>
                        {
                            if (result2 == MessageBoxResult.OK)
                            {
                                SetReturn(true);
                            }
                            else
                                SetReturn(false);
                        });
                    }
                    else
                      SetReturn(false);
                }
            });
        }

        /// <summary>
        /// 조회
        /// BIZ : BR_PRD_GET_PALLET_FOR_RETURN_NJ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if(cboLine.SelectedValue.Equals("SELECT"))
                Util.MessageValidation("SFU1223");
            GetPalletInfo();
            ClearLotInfoGrid();
        }

        private void btn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sPalletID = string.Empty;                
                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    // SFU1843 작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }               

                DataTable dtInfo = DataTableConverter.Convert(dgHist.ItemsSource);

                List<DataRow> drList = dtInfo.Select("CHK = 'True'").ToList();

                if (drList.Count <= 0)
                {
                    // SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                _tagPrintCount = drList.Count();

                foreach (DataRow drPrint in drList)
                {

                    if (drPrint["CLSS3_CODE"].ToString() == "MCC" || drPrint["CLSS3_CODE"].ToString() == "MCM")
                    {
                        TagPrint(drPrint);
                    }
                    else if (drPrint["CLSS3_CODE"].ToString() == "MCS")
                    {
                        TagPrint(drPrint);
                    }
                    else if (drPrint["CLSS3_CODE"].ToString() == "MCR")
                    {
                        TagPrint(drPrint);
                    }
                    else
                    {
                        POLYMER_TagPrint(drPrint);
                    }

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

        #region Method

        #region [BizCall]

        /// <summary>
        /// 반품유형 콤보 조회
        /// </summary>
        private void SetReturnTypeCombo()
        {
            try
            {
                // Data Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("CMCODE", typeof(string)); ;

                // Data Row
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CMCDTYPE"] = "RETURN_TYPE_CODE";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMMONCODE_FO", "INDATA", "OUTDATA", inTable);

                DataRow dr = dtResult.NewRow();
                dr["CBO_CODE"] = "SELECT";
                dr["CBO_NAME"] = "- SELECT -";
                dtResult.Rows.InsertAt(dr, 0);

                cboReturnType.DisplayMemberPath = "CBO_NAME";
                cboReturnType.SelectedValuePath = "CBO_CODE";
                cboReturnType.ItemsSource = dtResult.Copy().AsDataView();
                cboReturnType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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

                if (string.IsNullOrWhiteSpace(Util.NVC(cboReturnType.SelectedValue)) || Util.NVC(cboReturnType.SelectedValue) == "SELECT")
                {
                    // 반품유형을 선택해 주세요.
                    Util.MessageValidation("SFU3640");
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
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("RETURN_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("NOTIN", typeof(string));
                RQSTDT.Columns.Add("IN", typeof(string));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));

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
                    dr["EQSGID"] = string.IsNullOrEmpty(Util.NVC(cboLine.SelectedValue)) ? null : cboLine.SelectedValue;
                    dr["PKG_LOTID"] = txtLotID.Text;
                }

                dr["RETURN_TYPE_CODE"] = Util.NVC(cboReturnType.SelectedValue);

                DataTable dt = DataTableConverter.Convert(cboReturnType.ItemsSource);
                string attr1 = dt.Rows[cboReturnType.SelectedIndex]["ATTRIBUTE1"].ToString();

                if (Util.NVC(attr1).Equals("N"))
                {
                    // 일반(NORMAL) : 입고창고가 300R, 301R 이 아닌 반품 건
                    dr["NOTIN"] = "Y";
                }
                else
                {
                    // RMA : 입고창고가 300R, 301R 인 반품 건
                    dr["IN"] = "Y";
                }

                //출하처가 선택되었을때만 파라미터로 보냄. 무조건 보내면 파리미터 있고 값이 null 이라 where 문에 추가됨
                if (!string.IsNullOrEmpty(Util.NVC(popShipto.SelectedValue).Trim()))
                {
                    dr["SHIPTO_ID"] = Util.NVC(popShipto.SelectedValue).Trim();
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PALLET_FOR_RETURN_NJ", "INDATA", "OUTDATA", RQSTDT);
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
        /// 반품 Lot 상세 조회
        /// BIZ : BR_PRD_GET_LOT_FOR_RETURN_NJ
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

                foreach (int row in rowList)
                {
                    string sRcvIssID = Util.NVC(datagrid.GetCell(row, datagrid.Columns["RCV_ISS_ID"].Index).Value);
                    string sProdid = Util.NVC(datagrid.GetCell(row, datagrid.Columns["PRODID"].Index).Value);
                    iReqQty += Util.NVC_Int(datagrid.GetCell(row, datagrid.Columns["ISS_QTY2"].Index).Value);
                    string sStat = Util.NVC(datagrid.GetCell(row, datagrid.Columns["RCV_ISS_STAT_CODE"].Index).Value);

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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_LOT_FOR_RETURN_NJ", "INDATA", "OUTDATA", RQSTDT);

                if (!dtResult.Columns.Contains("YN"))
                {
                    DataColumn dc = new DataColumn("YN");
                    dc.DefaultValue = "N";
                    dtResult.Columns.Add(dc);
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
        /// BIZ : BR_PRD_REG_RETURN_PALLET_NJ
        /// </summary>
        /// 
        private void SetReturn(bool bRestore)
        {
            try
            {
                List<int> iReturnList = _util.GetDataGridCheckRowIndex(dgReturn, "CHK");

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                DataRow newRow = inDataTable.NewRow();
                inDataTable.Columns.Add("SHOPID");
                inDataTable.Columns.Add("EQSGID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("USERNAME");
                inDataTable.Columns.Add("SHFT_ID");
                inDataTable.Columns.Add("BOX_RESTORE_YN");
                
                newRow = inDataTable.NewRow();
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["EQSGID"] = cboLine.SelectedValue;
                newRow["USERID"] = txtWorker.Tag;
                newRow["USERNAME"] = txtWorker.Text;
                newRow["BOX_RESTORE_YN"] = bRestore? "Y" : "N";
                // newRow["SHFT_ID"] = txt.Text;
                inDataTable.Rows.Add(newRow);


                newRow = null;

                DataTable inReturnTable = indataSet.Tables.Add("INRETURN");
                inReturnTable.Columns.Add("RCV_ISS_ID");

              //  int iReqQty = 0;

                foreach (int row in iReturnList)
                {
                    //string sType = Util.NVC(dgReturn.GetCell(row, dgReturn.Columns["RCV_ISS_STAT_CODE"].Index).Value);
                    //if (sType != "SHIPPING")
                    //{
                    //    return;
                    //}
                    newRow = inReturnTable.NewRow();
                    newRow["RCV_ISS_ID"] = Util.NVC(dgReturn.GetCell(row, dgReturn.Columns["RCV_ISS_ID"].Index).Value);
                  //  iReqQty += Util.NVC_Int(dgReturn.GetCell(row, dgReturn.Columns["ISS_QTY2"].Index).Value);
                    inReturnTable.Rows.Add(newRow);
                }
                
                DataTable dtInfo = DataTableConverter.Convert(dgLotInfo.ItemsSource);

                newRow = null;

                DataTable inLotTable = indataSet.Tables.Add("INLOT");
                inLotTable.Columns.Add("RCV_ISS_ID");
                inLotTable.Columns.Add("BOXID");
                inLotTable.Columns.Add("LOTID");
                //  inLotTable.Columns.Add("PRDT_GRD_CODE");
                inLotTable.Columns.Add("CAPA_GRD_CODE");
                inLotTable.Columns.Add("RSST_GRD_CODE");
                inLotTable.Columns.Add("VLTG_GRD_CODE");
                inLotTable.Columns.Add("LOTQTY");
                inLotTable.Columns.Add("WIP_NOTE");

                foreach (DataRow dr in dtInfo.Rows)
                {
                    string sCapa = string.Empty;
                    string sRsst = string.Empty;
                    string sVltg = string.Empty;

                    newRow = inLotTable.NewRow();
                    newRow["RCV_ISS_ID"] = dr["RCV_ISS_ID"];
                    newRow["BOXID"] = dr["BOXID"];
                    newRow["LOTID"] = dr["LOTID"];
                    newRow["CAPA_GRD_CODE"] = sCapa = dr["CAPA_GRD_CODE"].ToString();
                    newRow["RSST_GRD_CODE"] = sRsst = dr["RSST_GRD_CODE"].ToString();
                    newRow["VLTG_GRD_CODE"] = sVltg = dr["VLTG_GRD_CODE"].ToString();
                    //   newRow["PRDT_GRD_CODE"] = sCapa + (string.IsNullOrEmpty(sRsst) ? "" : "-" + sRsst) + (string.IsNullOrEmpty(sVltg) ? "" : "-" + sVltg);
                    newRow["LOTQTY"] = dr["LOTQTY2"];
                    newRow["WIP_NOTE"] = dr["WIP_NOTE"];
                    inLotTable.Rows.Add(newRow);
                }
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RETURN_PALLET_LIST_NJ", "INDATA,INRETURN,INLOT", "OUTBOX,OUTPALLET", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult.Tables["OUTPALLET"].Rows.Count > 0)
                        {
                            if (bizResult.Tables["OUTPALLET"].Select("CART_SHEET_PRINT_YN = 'Y'").ToList().Count <= 0)
                            {
                                // SFU1275 정상 처리 되었습니다.
                                Util.MessageInfo("SFU1275");
                            }
                            else
                            {
                                for (int i = 0; i < bizResult.Tables["OUTPALLET"].Rows.Count; i++)
                                {
                                    if (Util.NVC(bizResult.Tables["OUTPALLET"].Rows[i]["CART_SHEET_PRINT_YN"]) == "Y")
                                    {
                                        string cartID = bizResult.Tables["OUTPALLET"].Rows[i]["PALLETID"].ToString();
                                        string eqptID = bizResult.Tables["OUTPALLET"].Rows[i]["EQPTID"].ToString();
                                        CartPrint(cartID, eqptID);
                                    }
                                }
                            }
                        }
                        else
                            // SFU1275 정상 처리 되었습니다.
                            Util.MessageInfo("SFU1275");

                        //if (string.Equals(prodType, "P"))
                        //{
                        //    string cartID = string.Empty;
                        //    string eqptID = string.Empty;
                        //    for (int i = 0; i < bizResult.Tables["OUTPALLET"].Rows.Count; i++)
                        //    {
                        //        cartID = bizResult.Tables["OUTPALLET"].Rows[i]["PALLETID"].ToString();
                        //        eqptID = bizResult.Tables["OUTPALLET"].Rows[i]["EQPTID"].ToString();
                        //        CartPrint(cartID, eqptID);
                        //    }
                        //}
                        //else
                        //{
                        //    string palletID = string.Empty;
                        //    int qty = 0;
                        //    for (int i = 0; i < bizResult.Tables["OUTBOX"].Rows.Count; i++)
                        //    {
                        //        palletID = bizResult.Tables["OUTBOX"].Rows[i]["BOXID"].ToString();
                        //        //qty = bizResult.Tables["OUTBOX"].Rows[i]["TOTAL_QTY"].ToString();
                        //        //TagPrint(palletID);


                        //    }
                        //}

                        GetPalletInfo();
                        ClearLotInfoGrid();
                        //  _util.SetDataGridCheck(dgReturn, "CHK", "RCV_ISS_ID", sRcvIssID);
                        //  DataTableConverter.SetValue(dgReturn.Rows[row].DataItem, "CHK", true);

                        //  GetLotInfo();

                        //for (int r= 0; r< dgLotInfo?.Rows?.Count - dgLotInfo?.Rows?.BottomRows?.Count; r++)
                        //{
                        //    string sPalletID = Util.NVC(dgLotInfo.GetCell(r, dgLotInfo.Columns["BOXID"].Index).Value);
                        //    string sSeq = Util.NVC(dgLotInfo.GetCell(r, dgLotInfo.Columns["WIPSEQ"].Index).Value);

                        //    PrintTag(sProdId, sPalletID, sSeq);
                        //}
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                }, indataSet);

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
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = "";
                Parameters[3] = Process.CELL_BOXING;
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
        private void SetCommCode(C1DataGrid dg, string comTypeCode, string attr1 = null)
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

                DataRow dr = dtResult.NewRow();
                dr["CBO_CODE"] = "";
                dr["CBO_NAME"] = " - SELECT-";
                dtResult.Rows.InsertAt(dr, 0);

                (dgLotInfo.Columns[comTypeCode] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult.Copy());

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }



        #endregion

        #endregion

        private void TagPrint(DataRow dr)
        {
            CMM_FORM_TAG_PRINT popupTagPrint = new CMM_FORM_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;
            if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5")) //남경
            {
                if (dr["CLSS3_CODE"].ToString() == "MCS") //초소형
                {
                    if (dr["WIP_QLTY_TYPE_CODE"].ToString() == "N")
                    {
                        popupTagPrint.DefectPalletYN = "Y";
                    }
                    else
                    {
                        popupTagPrint.QMSRequestPalletYN = "Y";
                    }
                }
                else
                {
                    if (dr["WIP_QLTY_TYPE_CODE"].ToString() == "N")
                    {
                        popupTagPrint.DefectPalletYN = "Y";
                    }
                    else
                    {
                        popupTagPrint.returnPalletYN = "Y";
                    }

                }

            }
            else
            {
                if (dr["WIP_QLTY_TYPE_CODE"].ToString() == "N")
                {
                    popupTagPrint.DefectPalletYN = "Y";
                }
                else
                {
                    popupTagPrint.QMSRequestPalletYN = "Y";
                }

            }
            popupTagPrint.PrintCount = _tagPrintCount.ToString();

            _tagPrintCount--;

            object[] parameters = new object[8];
            parameters[0] = dr["PROCID"];
            parameters[1] = null;              // 설비ID
            parameters[2] = dr["LOTID"];
            parameters[3] = dr["WIPSEQ"].ToString();
            parameters[4] = dr["WIPQTY"].ToString();
            parameters[5] = "N";                                         // 디스패치 처리
            parameters[6] = "Y";                                         // 출력 여부
            parameters[7] = (bool)chkTagPrint.IsChecked ? "N" : "Y";     // Direct 출력 여부

            C1WindowExtension.SetParameters(popupTagPrint, parameters);

            popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);
            grdMain.Children.Add(popupTagPrint);
            popupTagPrint.BringToFront();
        }

        private void popupTagPrint_Closed(object sender, EventArgs e)
        {
            CMM_FORM_TAG_PRINT popup = sender as CMM_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(popup);
        }

        private void POLYMER_TagPrint(DataRow dr)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupTagPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;
            popupTagPrint.CART_MERGE = "Y";
            popupTagPrint.PrintCount = _tagPrintCount.ToString();

            _tagPrintCount--;

            object[] parameters = new object[5];
            parameters[0] = "";       // _processCode;
            parameters[1] = null;     // Util.NVC(ComboEquipment.SelectedValue);
            parameters[2] = dr["CTNR_ID"];
            parameters[3] = (bool)chkTagPrint.IsChecked ? "N" : "Y";     // Direct 출력 여부
            parameters[4] = "N";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupTagPrint, parameters);

            popupTagPrint.Closed += new EventHandler(POLYMER_popupTagPrint_Closed);
            grdMain.Children.Add(popupTagPrint);
            popupTagPrint.BringToFront();
        }
        private void POLYMER_popupTagPrint_Closed(object sender, EventArgs e)
        {
            CMM_FORM_TAG_PRINT popup = sender as CMM_FORM_TAG_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(popup);
        }
        private void CartPrint(string cartID, string eqptID)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupCartPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupCartPrint.FrameOperation = this.FrameOperation;

            popupCartPrint.PrintCount = "1";

            object[] parameters = new object[5];
            parameters[0] = Process.CELL_BOXING_RETURN;
            parameters[1] = eqptID;
            parameters[2] = cartID;   // ButtonCertSelect.Tag;
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

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

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
                if (chkGroupSelect.IsChecked == true 
                    //&& (bool)DataTableConverter.GetValue(e.Row.DataItem, "CHK") 
                    && DataTableConverter.GetValue(e.Row.DataItem, "RCV_ISS_STAT_CODE").ToString() == "SHIPPING")
                {
                    for (int i = 0; i < datagrid.Rows.Count-1; i++)
                    {
                        if ((bool)DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "CHK") != (bool)DataTableConverter.GetValue(e.Row.DataItem, "CHK")
                            && DataTableConverter.GetValue(e.Row.DataItem, "PRODID").ToString() == DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "PRODID").ToString()
                            && DataTableConverter.GetValue(e.Row.DataItem, "RCV_ISS_STAT_CODE").ToString() == DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "RCV_ISS_STAT_CODE").ToString()
                            && DataTableConverter.GetValue(e.Row.DataItem, "EXP_DOM_TYPE_CODE").ToString() == DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "EXP_DOM_TYPE_CODE").ToString()
                            && DataTableConverter.GetValue(e.Row.DataItem, "TO_SLOC_ID").ToString() == DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "TO_SLOC_ID").ToString())
                        {
                            DataTableConverter.SetValue(datagrid.Rows[i].DataItem, "CHK", (bool)DataTableConverter.GetValue(e.Row.DataItem, "CHK"));
                        }
                    }
                }

                DataTable dtInfo = DataTableConverter.Convert(datagrid.ItemsSource);

                if (datagrid.Name == "dgReturn")
                {
                    if (dtInfo.Select("CHK = 'True'").ToList().Count > 0)
                    {
                        dtInfo = dtInfo.Select("CHK = 'True'").CopyToDataTable();
                        int cnt = dtInfo.DefaultView.ToTable(true, "PRODID").Rows.Count;
                        int cnt2 = dtInfo.DefaultView.ToTable(true, "FROM_SLOC_ID").Rows.Count; //반품창고
                        int cnt3 = dtInfo.DefaultView.ToTable(true, "TO_SLOC_ID").Rows.Count; //입고창고
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
                        if (cnt2 > 1)
                        {
                            // SFU1502	동일 반품창고가 아닙니다.
                            Util.MessageValidation("SFU4276");

                            if ((bool)DataTableConverter.GetValue(e.Row.DataItem, "CHK"))
                            {
                                DataTableConverter.SetValue(datagrid.Rows[e.Row.Index].DataItem, "CHK", false);
                                // e.Cancel = true;                           
                            }
                        }
                        if (cnt3 > 1)
                        {
                            // SFU1502	동일 반품창고가 아닙니다.
                            Util.MessageValidation("SFU4276");

                            if ((bool)DataTableConverter.GetValue(e.Row.DataItem, "CHK"))
                            {
                                DataTableConverter.SetValue(datagrid.Rows[e.Row.Index].DataItem, "CHK", false);
                                // e.Cancel = true;                           
                            }
                        }
                    }
                }
                else
                {
                    if (dtInfo.Select("CHK = 'True'").ToList().Count > 1)
                    {
                        // SFU2004	하나의 Pallet만 선택해주십시오.
                        Util.MessageValidation("SFU2004");

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
                    GetLotInfo(datagrid, datagrid.Name == "dgReturn" ? dgLotInfo : dgLotInfo3);
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

            DataTable dtCapa = DataTableConverter.Convert((dgLotInfo.Columns["CAPA_GRD_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource);
            DataTable dtRsst = DataTableConverter.Convert((dgLotInfo.Columns["RSST_GRD_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource);
            DataTable dtVolt = DataTableConverter.Convert((dgLotInfo.Columns["VLTG_GRD_CODE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource);

            int cnt_C = dtCapa.Select("CBO_CODE = '" + txtCapaGrd_Input.Text + "'").ToList().Count;
            int cnt_R = dtRsst.Select("CBO_CODE = '" + txtRsstGrd_Input.Text + "'").ToList().Count;
            int cnt_V = dtVolt.Select("CBO_CODE = '" + txtVltgGrd_Input.Text + "'").ToList().Count;

            if (cnt_C <= 0)
            {
                Util.MessageValidation("용량등급을 확인하세요.");
                return;
            }
            if (cnt_R <= 0)
            {
                Util.MessageValidation("저항등급을 확인하세요.");
                return;
            }
            if (cnt_V <= 0)
            {
                Util.MessageValidation("전압등급을 확인하세요.");
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
                dtInfo.Rows[i]["LOTID"] = txtLotId_Input.Text;
                dtInfo.Rows[i]["CAPA_GRD_CODE"] = txtCapaGrd_Input.Text;
                dtInfo.Rows[i]["RSST_GRD_CODE"] = txtRsstGrd_Input.Text;
                dtInfo.Rows[i]["VLTG_GRD_CODE"] = txtVltgGrd_Input.Text;
            }
            Util.GridSetData(dgLotInfo, dtInfo, FrameOperation, true);
        }

        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtInfo = DataTableConverter.Convert(dgLotInfo.ItemsSource);

            var list = dtInfo.AsEnumerable().GroupBy(g => new
            {
                LOTID = g.Field<string>("LOTID")
                                                    ,
                PRDT_GRD_CODE = g.Field<string>("PRDT_GRD_CODE")
            }
            ).Select(c => new { c.Key.LOTID, c.Key.PRDT_GRD_CODE, LOTQTY = c.Sum(s => Util.NVC_Int(s.Field<object>("LOTQTY2"))) }).ToList();

            DataTable dtNew = new DataTable();
            dtNew.Columns.Add("CHK");
            dtNew.Columns.Add("LOTID");
            dtNew.Columns.Add("PRDT_GRD_CODE");
            dtNew.Columns.Add("LOTQTY2", typeof(int));
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
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrWhiteSpace(txtPalletID2.Text))
                {
                    dr["BOXID"] = txtPalletID2.Text;
                }
                else
                {
                    dr["AREAID"] = string.IsNullOrWhiteSpace(Util.NVC(cboArea2.SelectedValue)) ? null : cboArea2.SelectedValue;
                    dr["FROM_DTTM"] = dtpDateFrom2.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                    dr["TO_DTTM"] = dtpDateTo2.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                    dr["PKG_LOTID"] = string.IsNullOrWhiteSpace(txtLotID2.Text) ? null : txtLotID2.Text;
                }

                //출하처가 선택되었을때만 파라미터로 보냄. 무조건 보내면 파리미터 있고 값이 null 이라 where 문에 추가됨
                if (!string.IsNullOrEmpty(Util.NVC(popShiptoHist.SelectedValue).Trim()))
                {
                    dr["SHIPTO_ID"] = Util.NVC(popShiptoHist.SelectedValue).Trim();
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_RETURN_PALLET_LIST_NJ", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult.Columns.Add("CHK");
                Util.GridSetData(dgHist, dtResult, FrameOperation, true);

                if (dgHist.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgHist.Columns["BOXID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                    DataGridAggregate.SetAggregateFunctions(dgHist.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
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

        private void btnReturnReg3_Click(object sender, RoutedEventArgs e)
        {

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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PALLET_FOR_RETURN_SUBCONTRACT_NJ", "INDATA", "OUTDATA", RQSTDT);
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

        private void btnWipRemarks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgHist.ItemsSource);

                List<DataRow> drList = dtInfo.Select("CHK = 'True'").ToList();

                if (drList.Count <= 0)
                {
                    // SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                BOX001_209_WIP_REMARKS popupWipRemarks = new BOX001_209_WIP_REMARKS();
                popupWipRemarks.FrameOperation = this.FrameOperation;

                object[] parameters = new object[1];
                parameters[0] = drList.ToArray();

                C1WindowExtension.SetParameters(popupWipRemarks, parameters);

                popupWipRemarks.Closed += new EventHandler(popupWipRemarks_Closed);

                grdMain.Children.Add(popupWipRemarks);
                popupWipRemarks.BringToFront();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }

        private void popupWipRemarks_Closed(object sender, EventArgs e)
        {
            BOX001_209_WIP_REMARKS popupWipRemarks = sender as BOX001_209_WIP_REMARKS;

            if (popupWipRemarks != null && popupWipRemarks.DialogResult == MessageBoxResult.OK)
            {
                btnSearch_Hist_Click(null, null);
            }
            this.grdMain.Children.Remove(popupWipRemarks);
        }
    }
}
