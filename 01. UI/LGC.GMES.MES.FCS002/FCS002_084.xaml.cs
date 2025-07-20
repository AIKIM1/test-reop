/*************************************************************************************
 Created Date : 2020.12.09
      Creator : 
   Decription : 개발/기술 Sample Cell 추출
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.09  NAME : Initial Created
  2022.03.29  강동희 : 조회조건추가-생산라인,모델,LotType
  2022.06.10  이정미 : 등록Tab 추출여부 Option Button 변경 (Default 자동 -> 수동)
  2022.08.18  조영대 : Loaded 이벤트 제거
  2022.11.16  조영대 : 이전 Tray ID 컬럼 추가
**************************************************************************************/
using C1.WPF; //20220329_조회조건추가-생산라인,모델,LotType
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_084 : UserControl, IWorkArea
    {

        public FCS002_084()
        {
            InitializeComponent();

            this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        Util _Util = new Util();
        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting
            InitCombo();

            dtpFromDate.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-2);                
            dtpToDate.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(1);


            chkAll.Checked += chkAll_Checked;
            chkAll.Unchecked += chkAll_UnChecked;

            this.Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            string[] sFilter = { "FORM_SMPL_TYPE_CODE", "Y", null, null, null, null };
            _combo.SetCombo(cboSampleType, CommonCombo_Form_MB.ComboStatus.SELECT, sFilter: sFilter, sCase: "CMN_WITH_OPTION");
            _combo.SetCombo(cboSearchSampleType, CommonCombo_Form_MB.ComboStatus.ALL, sFilter: sFilter, sCase: "CMN_WITH_OPTION");

            //20220329_조회조건추가-생산라인,모델,LotType START
            _combo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE");

            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent);

            // Lot 유형
            _combo.SetCombo(cboLotType, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LOTTYPE"); // 2021.08.19 Lot 유형 검색조건 추가
            //20220329_조회조건추가-생산라인,모델,LotType END
        }

        #endregion

        #region Event

        #region 등록
        private void btnInsertInputCell_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtInsertCellId.Text))
                return;
            if (rdoCell.IsChecked == true)
            {
                string sCellID = txtInsertCellId.Text.Trim();

                bool bRtn = GetCellInfo(sCellID);
                SetInit();
            }

            else
            {
                string sTrayID = txtInsertCellId.Text.Trim();

                if (GetTrayInfo(sTrayID) == false)
                    return;

            }

        }

        private void btnInsertClear_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgInsert);
            txtInsertCellCnt.Text = "0";
            txtInsertCellId.Text = string.Empty;
        }

        private void txtInsertCellId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtInsertCellId.Text.Trim() == string.Empty)
                    return;

                btnInsertInputCell_Click(null, null);
            }
        }

        private void txtInsertCellId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    if (rdoCell.IsChecked == true)
                    {
                        string[] stringSeparators = new string[] { "\r\n" };
                        string sPasteString = Clipboard.GetText();
                        string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                        string _ValueToFind = string.Empty;
                        bool bFlag = false;

                        if (sPasteStrings.Count() > 100)
                        {
                            Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                            return;
                        }

                        if (sPasteStrings[0].Trim() == "")
                        {
                            Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                            return;
                        }

                        for (int i = 0; i < sPasteStrings.Length; i++)
                        {
                            GetCellInfo(sPasteStrings[i].Trim());
                        }
                    }
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnInsertSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sSampleType = Util.GetCondition(cboSampleType);

                if (dgInsert.Rows.Count == 0)
                {
                    //등록할 대상이 존재하지 않습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0125"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtInsertCellId.SelectAll();
                            txtInsertCellId.Focus();
                        }
                    });
                    return;
                }

                if (string.IsNullOrEmpty(sSampleType) || sSampleType.Equals("SELECT"))
                {
                    //Sample 유형을 선택해주세요.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0310"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtInsertCellId.SelectAll();
                            txtInsertCellId.Focus();
                        }
                    });
                    return;
                }

                Insert(sSampleType);

                // ----------수동자동 제거-----
                //if (rdoManual.IsChecked == true)
                //{
                //    //(배출여부 : 수동)은 Cell을 추출한 이후에만 선택하셔야 됩니다. 
                //    //수동으로 선택할 경우 Selector에서 Cell이 빠지지 않습니다. (수동)선택을 유지하시겠습니까?
                //    Util.MessageConfirm("FM_ME_0345", (result) =>
                //    {
                //        if (result != MessageBoxResult.OK)
                //        {
                //            rdoAuto.IsChecked = true;
                //        }
                //        else
                //        {
                //            Insert(sSampleType);
                //        }
                //    });
                //}
                //else
                //{
                //    Insert(sSampleType);
                //}

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void Insert(string sSampleType)
        {
            //저장하시겠습니까?
            Util.MessageConfirm("FM_ME_0214", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    string BizRuleID = "BR_SET_SMPL_CELL_ALL_MB";

                    // ---폐기대기 제거 --
                    //if ((bool)rdoGood.IsChecked)
                    //{
                    //    BizRuleID = "BR_SET_SMPL_CELL_ALL_MB";
                    //}
                    //else
                    //{
                    //    BizRuleID = "BR_SET_NGLOT_SMPL_CELL_MB";
                    //}

                    DataSet indataSet = new DataSet();
                    DataTable dtIndata = indataSet.Tables.Add("INDATA");
                    dtIndata.Columns.Add("USERID", typeof(string));
                    dtIndata.Columns.Add("TD_FLAG", typeof(string));
                    dtIndata.Columns.Add("SPLT_FLAG", typeof(string));

                    DataTable dtInCell = indataSet.Tables.Add("INCELL");
                    dtInCell.Columns.Add("SUBLOTID", typeof(string));
                    dtInCell.Columns.Add("UNPACK_CELL_YN", typeof(string));

                    DataRow InRow = dtIndata.NewRow();
                    InRow["USERID"] = LoginInfo.USERID;
                    InRow["TD_FLAG"] = sSampleType;
                    InRow["SPLT_FLAG"] =  "Y"; // 자동 수동 제거 , 수동으로만 작동
                    //InRow["SPLT_FLAG"] = (rdoAuto.IsChecked == true) ? "N" : "Y";

                    dtIndata.Rows.Add(InRow);

                    for (int i = 0; i < dgInsert.Rows.Count; i++)
                    {
                        DataRow RowCell = dtInCell.NewRow();

                        string sSublotID = Util.NVC(DataTableConverter.GetValue(dgInsert.Rows[i].DataItem, "SUBLOTID"));
                 
                        RowCell["SUBLOTID"] = Util.Convert_CellID(sSublotID);

                        RowCell["UNPACK_CELL_YN"] = Util.NVC(DataTableConverter.GetValue(dgInsert.Rows[i].DataItem, "UNPACK_CELL_YN"));
                        // 폐기대기 제거
                        //if ((bool)rdoGood.IsChecked)
                        //{
                        //    RowCell["UNPACK_CELL_YN"] = Util.NVC(DataTableConverter.GetValue(dgInsert.Rows[i].DataItem, "UNPACK_CELL_YN"));
                        //}
                        dtInCell.Rows.Add(RowCell);
                    }

                    ShowLoadingIndicator();
                    new ClientProxy().ExecuteService_Multi(BizRuleID, "INDATA,INCELL", "OUTDATA", (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                            {
                                //저장하였습니다.
                                Util.MessageInfo("FM_ME_0215");
                                dgInsert.ItemsSource = null;
                            }
                            else
                            {
                                //저장실패하였습니다.
                                Util.MessageInfo("FM_ME_0213");
                            }
                        }
                        catch (Exception ex)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        }
                        finally
                        {
                            HiddenLoadingIndicator();
                        }
                    }, indataSet);
                }
            });
        }

        private void btnDelCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

                if (presenter == null)
                    return;

                int clickedIndex = presenter.Row.Index;
                DataTable dt = DataTableConverter.Convert(dgInsert.ItemsSource);
                dt.Rows.RemoveAt(clickedIndex);
                Util.GridSetData(presenter.DataGrid, dt, this.FrameOperation);

                txtInsertCellCnt.Text = (int.Parse(txtInsertCellCnt.Text) - 1).ToString();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgInsert_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }


        #endregion

        #region 조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            chkAll.IsChecked = false;
            GetList();
        }
        private void dgSearch_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("CSTID") || e.Cell.Column.Name.Equals("CSTID_PV"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }

                if (e.Cell.Column.Name.Equals("SMPL_STAT"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SMPL_STAT")).Equals(ObjectDic.Instance.GetObjectName("RESTORE")))
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Cell.Row.Index, 0);
                        cell.Presenter.IsEnabled = false;
                    }
                }

                if (e.Cell.Column.Name.Equals("CHK"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DFCT_SLOC_SMPL_FLAG")).Equals("Y")) //불량창고의 셀일 경우
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Cell.Row.Index, 0);
                        cell.Presenter.IsEnabled = false;
                    }
                }
            }));
        }

        private void dgSearch_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void btnSearchRecover_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 2021-05-13 grid Check 확인 로직 오류로 제거
                /*int iRCVCnt = 0;

                 for (int i = 0; i < dgSearch.Rows.Count; i++)
                  {
                      if (Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "CHK")).Equals("True") || Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "CHK")).Equals("0"))
                      {
                          iRCVCnt++;
                      }
                  }
                */
                if (!dgSearch.IsCheckedRow("CHK"))
                {
                    //복구 대상이 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0139"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                        }
                    });
                    return;
                }

                //복구하시겠습니까?
                Util.MessageConfirm("FM_ME_0141", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataSet indataSet = new DataSet();
                        DataTable dtIndata = indataSet.Tables.Add("INDATA");
                        dtIndata.Columns.Add("USERID", typeof(string));
                        dtIndata.Columns.Add("TD_FLAG", typeof(string));

                        DataTable dtInCell = indataSet.Tables.Add("INCELL");
                        dtInCell.Columns.Add("SUBLOTID", typeof(string));

                        DataRow InRow = dtIndata.NewRow();
                        InRow["USERID"] = LoginInfo.USERID;
                        InRow["TD_FLAG"] = "C";

                        dtIndata.Rows.Add(InRow);

                        for (int i = 0; i < dgSearch.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "CHK")).Equals("True") || Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "CHK")).Equals("1"))
                            {
                                DataRow RowCell = dtInCell.NewRow();

                                string sublot = Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "SUBLOTID"));
                                RowCell["SUBLOTID"] = Util.Convert_CellID(sublot);
                                dtInCell.Rows.Add(RowCell);
                            }
                        }

                        ShowLoadingIndicator();
                        new ClientProxy().ExecuteService_Multi("BR_SET_SMPL_CELL_MB", "INDATA,INCELL", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                btnSearch_Click(null, null);

                            }
                            catch (Exception ex)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, indataSet);
                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void dgSearch_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell == null || datagrid.CurrentRow == null)
                {
                    return;
                }

                if (datagrid.CurrentColumn.Name.Equals("CSTID") || datagrid.CurrentColumn.Name.Equals("CSTID_PV"))
                {
                    FCS002_021 wndRunStart = new FCS002_021();
                    wndRunStart.FrameOperation = FrameOperation;

                    if (wndRunStart != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = cell.Text;

                        this.FrameOperation.OpenMenu("SFU010710300", true, Parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgSearch_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion

        #region 복구
        private void btnRecoverInputCell_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Util.NVC(txtRecoverCellId.Text)))
            {
                GetRecoverCellInfo(Util.NVC(txtRecoverCellId.Text));
            }
        }

        private void btnRecoverClear_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgSearch);
            txtRecoverCellCnt.Text = "0";
            txtRecoverCellId.Text = string.Empty;
        }

        private void btnRecoverSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRecover.Rows.Count == 0)
                {
                    //복구 대상이 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0139"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtInsertCellId.SelectAll();
                            txtInsertCellId.Focus();
                        }
                    });
                    return;
                }

                //복구하시겠습니까?
                Util.MessageConfirm("FM_ME_0141", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataSet indataSet = new DataSet();
                        DataTable dtIndata = indataSet.Tables.Add("INDATA");
                        dtIndata.Columns.Add("USERID", typeof(string));
                        dtIndata.Columns.Add("TD_FLAG", typeof(string));

                        DataTable dtInCell = indataSet.Tables.Add("INCELL");
                        dtInCell.Columns.Add("SUBLOTID", typeof(string));

                        DataRow InRow = dtIndata.NewRow();
                        InRow["USERID"] = LoginInfo.USERID;
                        InRow["TD_FLAG"] = "C";

                        dtIndata.Rows.Add(InRow);

                        for (int i = 0; i < dgRecover.Rows.Count; i++)
                        {
                            DataRow RowCell = dtInCell.NewRow();
                            string sublot = Util.NVC(DataTableConverter.GetValue(dgRecover.Rows[i].DataItem, "SUBLOTID"));
                            RowCell["SUBLOTID"] = Util.Convert_CellID(sublot);
                            dtInCell.Rows.Add(RowCell);
                        }

                        ShowLoadingIndicator();
                        new ClientProxy().ExecuteService_Multi("BR_SET_SMPL_CELL_MB", "INDATA,INCELL", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                                {
                                    //복구 완료하였습니다.
                                    Util.MessageInfo("FM_ME_0140");
                                    dgRecover.ItemsSource = null;

                                }
                                else
                                {
                                    //복구실패하였습니다.
                                    Util.MessageInfo("FM_ME_0311");
                                }
                            }
                            catch (Exception ex)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, indataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void txtRecoverCellId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!string.IsNullOrEmpty(Util.NVC(txtRecoverCellId.Text)))
                {
                    btnRecoverInputCell_Click(null, null);
                }
            }
        }

        private void txtRecoverCellId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    string _ValueToFind = string.Empty;
                    bool bFlag = false;

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    if (sPasteStrings[0].Trim() == "")
                    {
                        Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                        return;
                    }

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        GetRecoverCellInfo(sPasteStrings[i].Trim());
                    }
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnRDelCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                #region MyRegion
                //Button btnRDelCell = sender as Button;
                //DataTable dt = DataTableConverter.Convert(dgRecover.ItemsSource);
                //if (btnRDelCell != null)
                //{
                //    DataRowView dataRow = ((FrameworkElement)sender).DataContext as DataRowView;

                //    if (string.Equals(btnRDelCell.Name, "btnRDelCell"))
                //    {
                //        DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

                //        if (presenter == null)
                //            return;

                //        int clickedIndex = presenter.Row.Index;
                //        dt.Rows.RemoveAt(clickedIndex);
                //        Util.GridSetData(presenter.DataGrid, dt, this.FrameOperation);

                //        txtRecoverCellCnt.Text = dt.Rows.Count.ToString();
                //    }
                //} 
                #endregion

                DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

                if (presenter == null)
                    return;

                int clickedIndex = presenter.Row.Index;
                DataTable dt = DataTableConverter.Convert(dgRecover.ItemsSource);
                dt.Rows.RemoveAt(clickedIndex);
                Util.GridSetData(presenter.DataGrid, dt, this.FrameOperation);

                txtRecoverCellCnt.Text = (int.Parse(txtRecoverCellCnt.Text) - 1).ToString();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgRecover_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index <= dataGrid.Rows.Count - 1)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion

        #endregion

        #region Method

        private bool GetCellInfo(string CellID)
        {
            try
            {
                //string sCellID = string.Empty;


                string sCellID = Util.Convert_CellID(CellID);

                if (string.IsNullOrEmpty(sCellID))
                {
                    return false;
                }

                //스프레드에 있는지 확인
                for (int i = 0; i < dgInsert.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgInsert.Rows[i].DataItem, "SUBLOTID").ToString() == sCellID)
                    {
                        //목록에 기존재하는 Cell 입니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0132", sCellID), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                SetInit();
                            }
                        });
                        return false;
                    }
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("SPLT_FLAG", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = Util.NVC(sCellID);
                dr["SPLT_FLAG"] = "N";
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                string BizRule = "DA_SEL_CELL_SAMPLE_YN_MB";

                // ---폐기대기 제거 ----
                //if ((bool)rdoGood.IsChecked)
                //{
                //    BizRule = "DA_SEL_CELL_SAMPLE_YN_MB";
                //}
                //else
                //{
                //    BizRule = "DA_SEL_CELL_SAMPLE_YN_NG_MB";
                //}

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(BizRule, "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dgInsert.ItemsSource);

                    if (dt.Rows.Count > 0)
                    {
                        DataRow dr1 = dt.NewRow();
                        dr1["CHK"] = Util.NVC(dtRslt.Rows[0]["CHK"]);
                        dr1["SUBLOTID"] = Util.NVC(dtRslt.Rows[0]["SUBLOTID"]);
                        dr1["CANID"] = Util.NVC(dtRslt.Rows[0]["CANID"]);
                        dr1["VENTID"] = Util.NVC(dtRslt.Rows[0]["VENTID"]);
                        dr1["CSTID"] = Util.NVC(dtRslt.Rows[0]["CSTID"]);
                        dr1["ROUTID"] = Util.NVC(dtRslt.Rows[0]["ROUTID"]);
                        dr1["PROD_LOTID"] = Util.NVC(dtRslt.Rows[0]["PROD_LOTID"]);
                        if (!string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[0]["CSTSLOT"]))) dr1["CSTSLOT"] = Util.NVC_Decimal(dtRslt.Rows[0]["CSTSLOT"]);
                        dr1["ROUT_NAME"] = Util.NVC(dtRslt.Rows[0]["ROUT_NAME"]);
                        dr1["LOTID"] = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                        dr1["EQSGID"] = Util.NVC(dtRslt.Rows[0]["EQSGID"]);
                        //2021.07.05 컬럼 추가
                        dr1["SUBLOTJUDGE"] = Util.NVC(dtRslt.Rows[0]["SUBLOTJUDGE"]);
                        dr1["LOT_DETL_TYPE_CODE"] = Util.NVC(dtRslt.Rows[0]["LOT_DETL_TYPE_CODE"]);
                        dr1["DFCT_YN"] = Util.NVC(dtRslt.Rows[0]["DFCT_YN"]);
                       // if ((bool)rdoGood.IsChecked) // 폐기대기 제거
                         dr1["UNPACK_CELL_YN"] = Util.NVC(dtRslt.Rows[0]["UNPACK_CELL_YN"]); //양품일 경우 포장 대기 cell 구분

                        dt.Rows.Add(dr1);
                        
                        Util.GridSetData(dgInsert, dt, FrameOperation, true);
                        txtInsertCellCnt.Text = (int.Parse(txtInsertCellCnt.Text) + dtRslt.Rows.Count).ToString();
                    }
                    else
                    {
                        Util.GridSetData(dgInsert, dtRslt, FrameOperation, true);
                        txtInsertCellCnt.Text = dtRslt.Rows.Count.ToString();
                    }
                }
                else
                {
                    //[Cell ID : {0}]의 정보가 존재하지 않거나, 이미 추출된 Cell 입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0308", sCellID), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            //SetInit();
                        }
                    });
                    return false;
                }


                //SetInit();
                return true;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool GetTrayInfo(string TrayID)
        {
            try
            {


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("INPUTID", typeof(string));
                RQSTDT.Columns.Add("GOOD_FLAG", typeof(string));
                RQSTDT.Columns.Add("INBOX_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["INPUTID"] = txtInsertCellId.Text;
                dr["GOOD_FLAG"] = "Y"; // 폐기대기 제거
                dr["INBOX_FLAG"] = rdoInbox.IsChecked == true ? "Y" : "N"; // INBOX 여부
                //dr["GOOD_FLAG"] = rdoGood.IsChecked == true ? "Y" : "N";
                RQSTDT.Rows.Add(dr);

                DataSet inDataSet = new DataSet();
                inDataSet.Tables.Add(RQSTDT);

                DataTable SublotList = new DataTable();
                //TC_CELL_SCRAP테이블 삭제하고, SUBLOT테이블의 SUBLOTSCRAP을 일단 사용하는걸로.. 2020/12/14 WITH 정종덕
                //DataSet SearchTray = new ClientProxy().ExecuteServiceSync("BR_GET_CELL_SAMPLE_YN_TRAY_MB", "INDATA", "OUTDATA,OUT_CELLDATA", RQSTDT);
                DataSet SearchTray = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_CELL_SAMPLE_YN_TRAY_MB", "INDATA", "OUTDATA,OUT_CELLDATA", inDataSet);

                //SearchTray.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("1")
                if (SearchTray.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("-1"))
                {
                    Util.AlertInfo("FM_ME_0185"); // 처리오류
                    return false;
                }
                else if (SearchTray.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("1"))
                {
                    Util.AlertInfo("FM_ME_0529"); // 샘플 추출 가능 tray 아님 
                    return false;
                }
                else if (SearchTray.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("2"))
                {
                    Util.AlertInfo("FM_ME_0531"); // 샘플 추출 가능 box 아님 
                    return false;
                }

                DataTable dt = DataTableConverter.Convert(dgInsert.ItemsSource);

                dt.Merge(SearchTray.Tables["OUT_CELLDATA"]);

                dt = dt.DefaultView.ToTable(true);

                //Util.GridSetData(dgInsert, SearchTray.Tables["OUT_CELLDATA"], FrameOperation, true);
                Util.GridSetData(dgInsert, dt, FrameOperation, true);
                txtInsertCellCnt.Text = dt.Rows.Count.ToString();
                return true;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        private void GetRecoverCellInfo(string CellID)
        {
            try
            {

                string sCellID = Util.Convert_CellID(CellID);

                //스프레드에 있는지 확인
                for (int i = 0; i < dgRecover.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgRecover.Rows[i].DataItem, "SUBLOTID").ToString() == sCellID)
                    {
                        //목록에 기존재하는 Cell 입니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0132"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                SetInit();
                            }
                        });
                        return;
                    }
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("SPLT_FLAG", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = Util.NVC(sCellID);
                dr["SPLT_FLAG"] = "Y";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_RECOVER_MB", "RQSTDT", "RSLTDT", dtRqst);
                if (dtRslt.Rows.Count > 0)
                {
                    DataTable dt = DataTableConverter.Convert(dgRecover.ItemsSource);

                    if (dt.Rows.Count > 0)
                    {
                        DataRow dr1 = dt.NewRow();
                        dr1["SUBLOTID"] = Util.NVC(dtRslt.Rows[0]["SUBLOTID"]);
                        dr1["CSTID"] = Util.NVC(dtRslt.Rows[0]["CSTID"]);
                        dr1["ROUTID"] = Util.NVC(dtRslt.Rows[0]["ROUTID"]);
                        dr1["PROD_LOTID"] = Util.NVC(dtRslt.Rows[0]["PROD_LOTID"]);
                        if (!string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[0]["CSTSLOT"])))
                        {
                            dr1["CSTSLOT"] = Util.NVC_Decimal(dtRslt.Rows[0]["CSTSLOT"]);
                        }
                        dr1["ROUT_NAME"] = Util.NVC(dtRslt.Rows[0]["ROUT_NAME"]);
                        dr1["LOTID"] = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                        dr1["EQSGID"] = Util.NVC(dtRslt.Rows[0]["EQSGID"]);

                        // 2025.05.26 |csspoto| : OutData 누락건 추가
                        dr1["CANID"] = Util.NVC(dtRslt.Rows[0]["CANID"]);
                        dr1["VENTID"] = Util.NVC(dtRslt.Rows[0]["VENTID"]);
                        dr1["SUBLOTJUDGE"] = Util.NVC(dtRslt.Rows[0]["SUBLOTJUDGE"]);

                        dt.Rows.Add(dr1);
                        Util.GridSetData(dgRecover, dt, FrameOperation, true);
                        txtRecoverCellCnt.Text = (int.Parse(txtRecoverCellCnt.Text) + dtRslt.Rows.Count).ToString();
                    }
                    else
                    {
                        Util.GridSetData(dgRecover, dtRslt, FrameOperation, true);
                        txtRecoverCellCnt.Text = dtRslt.Rows.Count.ToString();
                    }
                }
                else
                {
                    //[Cell ID : {0}]은 추출된 Cell이 아닙니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0309", sCellID), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            RcvInit();
                        }
                    });
                    return;
                }

                RcvInit();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("SMPL_TYPE_CODE", typeof(string));
                //20220329_조회조건추가-생산라인,모델,LotType START
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));
                //20220329_조회조건추가-생산라인,모델,LotType END

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd");
                if (!string.IsNullOrEmpty(txtSearchCellId.Text))
                {

                    string sCellID = Util.Convert_CellID(txtSearchCellId.Text);

                    dr["SUBLOTID"] = sCellID;
                }

                dr["SMPL_TYPE_CODE"] = Util.GetCondition(cboSearchSampleType, bAllNull: true);
                //20220329_조회조건추가-생산라인,모델,LotType START
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["LOTTYPE"] = Util.GetCondition(cboLotType, bAllNull: true);
                //20220329_조회조건추가-생산라인,모델,LotType END
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SAMPLE_CELL_MB", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgSearch, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetInit()
        {
            txtInsertCellId.SelectAll();
            txtInsertCellId.Focus();
        }

        private void RcvInit()
        {
            txtRecoverCellId.SelectAll();
            txtRecoverCellId.Focus();
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
        private void rdoLotType_Click(object sender, RoutedEventArgs e)
        {
            //Util.gridClear(dgInsert);

            //if (sender == null || e == null) return;
            //RadioButton rdo = sender as RadioButton;

            //if (rdo.Name.Equals(rdoNG.Name))
            //{
            //    rdoManual.IsChecked = true;
            //    rdoAuto.IsEnabled = false;
            //}
            //else
            //{
            //    rdoAuto.IsChecked = true;
            //    rdoAuto.IsEnabled = true;
            //}
        }
        #endregion

        private void rdoCell_Checked(object sender, RoutedEventArgs e)
        {
            if (btnInsertSave != null)
            {
                if (rdoCell.IsChecked == true)
                {
                    txtSample.Text = ObjectDic.Instance.GetObjectName("SAMPLING_CELL");

                }
                else if (rdoTray.IsChecked == true)
                {
                    txtSample.Text = ObjectDic.Instance.GetObjectName("SAMPLING_TRAY");
                } 
                else
                {
                    txtSample.Text = ObjectDic.Instance.GetObjectName("SAMPLING_INBOX");
                }
            }
        }

        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgSearch.Rows.Count; i++)
            {
                if(!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "CSTID_PV"))))
                     DataTableConverter.SetValue(dgSearch.Rows[i].DataItem, "CHK", true);
            }
        }

        private void chkAll_UnChecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgSearch.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgSearch.Rows[i].DataItem, "CHK", false);
            }
        }
    }
}
