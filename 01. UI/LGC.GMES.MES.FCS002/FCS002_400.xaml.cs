/*************************************************************************************
 Created Date :
      Creator : 
   Decription : Cell 추적 설비 정보관리
--------------------------------------------------------------------------------------
 [Change History]         CSR NO   
  2023.09.08     DEV      SI           Initial Coding(FCS002_022 에서 복제)
  2024.02.21     이홍주   NFF 양산     Pallet 포장완료시 Cell교체, 방습제 교체 버튼 Disabled
  2024.03.20     이홍주   NFF 양산     Column방향 실물과 일치 시킴(H,G,F,E,D,C,B,A 순)
  2024.04.18     이홍주   NFF 양산     BOX정보 구역에 현재 조회중인 BOXID 정보 표현
  2024.04.22     이홍주   NFF 양산     NG 셀 표현 추가  
  2024.05.09     이홍주   NFF 양산     NoRead(NG) Cell 정보 삭제기능추가
  2024.06.28     이홍주   NFF 양산     불량셀 삭제 버튼 숨김처리
  2024.07.19     이홍주   NFF 양산     CELL 교체 BIZ 수정
  2024.07.25     이홍주   NFF 양산     엑셀 칼럼 방향과 실물 일치 시킴

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// FCS002_400.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_400 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string _sublotId;
        private string _BaseCellID;
        private int _BaseCellNo;
        const int alphabet_count = 26;
        private DataTable _dtColor = new DataTable();
        private DataTable _dtOutDataSublot;
        private DataTable _dtOutDataBox;

        #region [테스트 데이터]
        private int[,] gridSize = new int[5, 2] { { 16, 16 }, { 20, 20 }, { 15, 15 }, { 10, 10 }, { 25, 25 } };
        private int index = 0;
        #endregion

        public string BASECELLID
        {
            set { this._BaseCellID = value; }
            get { return this._BaseCellID; }
        }

        public int BASECELLNO
        {
            set { this._BaseCellNo = value; }
            get { return this._BaseCellNo; }
        }
        #endregion

        #region [Initialize]
        public FCS002_400()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            txtCellID.IsReadOnly = false;
            txtCellID.Text = string.Empty;

            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거

            InitLegend();

            rdoSublotId.IsChecked = true;
        }
        #endregion

        #region [Method]

        private void InitLegend()
        {
            try
            {
                C1ComboBoxItem cbItemTiTle = new C1ComboBoxItem { Content = ObjectDic.Instance.GetObjectName("- LEGEND -") };

                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                //dr["CMCDTYPE"] = "FORM_SUBLOTSTATUS_MCC";
                //RQSTDT.Rows.Add(dr);

                //ShowLoadingIndicator();
                //_dtColor = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CMN_ALL_ITEMS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                //if(_dtColor.Rows.Count == 0 || _dtColor == null)
                //{
                //_dtColor.Rows.Add("DUPCELL",    "셀 중복",    "LemonChiffon",  "Black", null, null, null);  2024.05 제외
                //_dtColor.Rows.Add("MIXGRADE",   "등급 섞임",  "Blue",          "White", null, null, null); 2024.05 제외

                _dtColor.Columns.Add("CBO_CODE", typeof(string));
                _dtColor.Columns.Add("CBO_NAME", typeof(string));
                _dtColor.Columns.Add("ATTRIBUTE1", typeof(string));
                _dtColor.Columns.Add("ATTRIBUTE2", typeof(string));
                _dtColor.Columns.Add("ATTRIBUTE3", typeof(string));
                _dtColor.Columns.Add("ATTRIBUTE4", typeof(string));
                _dtColor.Columns.Add("ATTRIBUTE5", typeof(string));

                _dtColor.Rows.Add("MIXLOT", "LOT 혼입", "Red", "White", null, null, null);
                _dtColor.Rows.Add("NG", "NG 셀", "Thistle", "Black", null, null, null);
                _dtColor.Rows.Add("NOCELL", "셀 없음", "Silver", "Black", null, null, null);
                _dtColor.Rows.Add("NOREAD", "BCR 실패", "LightGreen", "Black", null, null, null);
                _dtColor.Rows.Add("NORMAL", "정상", "White", "Black", null, null, null);
                //}


                foreach (DataRow row in _dtColor.Rows)
                {
                    if (row["ATTRIBUTE1"].ToString().IsNullOrEmpty())
                    {
                        continue;
                    }

                    C1ComboBoxItem cbItem = new C1ComboBoxItem
                    {
                        Content = row["CBO_NAME"].ToString(),
                        Background = new BrushConverter().ConvertFromString(row["ATTRIBUTE1"].ToString()) as SolidColorBrush,
                        Foreground = new BrushConverter().ConvertFromString(row["ATTRIBUTE2"].ToString()) as SolidColorBrush
                    };
                    //cboColorLegend.Items.Add(cbItem);
                }


                // cboColorLegend.SelectedIndex = 0;

                //-----------------------------------------------------
                //  CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

                //  string[] filter = { "1", _LANEID };
                //  _combo.SetCombo(cboOperLegend, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "CMN", sFilter: new string[] { "FORMATION_STATUS_NEXT_PROC","11,12,13,14,B1,31,41,51" });

                //------------------------------------------------------

                // 색 범례 
                SetColorGrid(_dtColor);

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

        private void SetColorGrid(DataTable dt)
        {

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                C1DataGrid dgNew = new C1DataGrid();

                C1.WPF.DataGrid.DataGridTextColumn textColumn1 = new C1.WPF.DataGrid.DataGridTextColumn();
                textColumn1.Header = "Color";
                textColumn1.Binding = new Binding("CBO_NAME");
                textColumn1.HorizontalAlignment = HorizontalAlignment.Center;
                textColumn1.IsReadOnly = true;
                textColumn1.Width = new C1.WPF.DataGrid.DataGridLength(100, DataGridUnitType.Pixel);

                C1.WPF.DataGrid.DataGridTextColumn textColumn2 = new C1.WPF.DataGrid.DataGridTextColumn();
                textColumn2.Header = "Color";
                textColumn2.Binding = new Binding("ATTRIBUTE1");
                textColumn2.HorizontalAlignment = HorizontalAlignment.Center;
                textColumn2.IsReadOnly = true;
                textColumn2.Visibility = Visibility.Collapsed;

                C1.WPF.DataGrid.DataGridTextColumn textColumn3 = new C1.WPF.DataGrid.DataGridTextColumn();
                textColumn3.Header = "Color";
                textColumn3.Binding = new Binding("ATTRIBUTE2");
                textColumn3.HorizontalAlignment = HorizontalAlignment.Center;
                textColumn3.IsReadOnly = true;
                textColumn3.Visibility = Visibility.Collapsed;

                dgNew.Columns.Add(textColumn1);
                dgNew.Columns.Add(textColumn2);
                dgNew.Columns.Add(textColumn3);

                // dgNew.IsEnabled = false;
                // dgNew.IsReadOnly = true;
                dgNew.HeadersVisibility = C1.WPF.DataGrid.DataGridHeadersVisibility.None;
                dgNew.FrozenColumnCount = 0;
                dgNew.SelectionMode = C1.WPF.DataGrid.DataGridSelectionMode.SingleRow;
                dgNew.LoadedCellPresenter += dgColor_LoadedCellPresenter;
                dgNew.SelectedBackground = null;


                Grid.SetRow(dgNew, 0);
                Grid.SetColumn(dgNew, i + 1);

                dgColor.Children.Add(dgNew);

                DataTable dtRow = new DataTable();
                dtRow.Columns.Add("CBO_NAME", typeof(string));
                dtRow.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRow.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow drRow = dtRow.NewRow();
                drRow["CBO_NAME"] = dt.Rows[i]["CBO_NAME"];
                drRow["ATTRIBUTE1"] = dt.Rows[i]["ATTRIBUTE1"];
                drRow["ATTRIBUTE2"] = dt.Rows[i]["ATTRIBUTE2"];
                dtRow.Rows.Add(drRow);

                Util.GridSetData(dgNew, dtRow, FrameOperation, false);
            }
        }

        #region 조회 2 : BOX CELL 의 제품 정보 + CELLs In BOX  정보

        #endregion
        #endregion

        #region [Event]

        private void dgColor_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(e.Cell.Column.Name) == "CBO_NAME")
                    {
                        e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Background = new BrushConverter().ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ATTRIBUTE1").ToString()) as SolidColorBrush;
                        e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Foreground = new BrushConverter().ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ATTRIBUTE2").ToString()) as SolidColorBrush;
                    }
                }
            }));
        }

        private void txtCellID_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtCellID.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
        }

        private void txtBoxID_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtBoxID.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
        }


        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void rdo_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rbClick = (RadioButton)sender;
            if (_dtOutDataBox != null && _dtOutDataBox.Rows.Count > 0)
                SetInBoxData(_dtOutDataBox, _dtOutDataSublot);
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void DataGridColAdd(C1.WPF.DataGrid.C1DataGrid dg, int colCount)
        {
            List<String> labels = new List<String>();

            int modVal, divVal;

            for (int i = 0; i < colCount; i++)
            {
                divVal = (i / alphabet_count);
                modVal = ((i + 1) == alphabet_count) ? alphabet_count : ((i + 1) % alphabet_count);

                // ASCII TABLE에서 'A' 문자 앞에 있는 코드가 '@' 문자임.
                labels.Add(((divVal > 0) ? (Convert.ToChar('@' + divVal)).ToString() : String.Empty) + (Convert.ToChar('@' + modVal)).ToString());
            }


            dg.Columns.Clear();

            foreach (string label in labels)
            {
                C1.WPF.DataGrid.DataGridTextColumn column = new C1.WPF.DataGrid.DataGridTextColumn();
                column.Header = label;
                column.Width = new C1.WPF.DataGrid.DataGridLength((dg.ActualWidth / colCount));

                dg.Columns.Add(column);
            }
        }


        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int rowCount)
        {
            try
            {
                // 여러건 추가 시 안되는 부분 확인
                DataTable dt = new DataTable();

                int addRows = 0;
                if (Math.Abs(rowCount) > 0)
                {
                    if (rowCount + dg.Rows.Count > 576)
                    {
                        // 최대 ROW수는 576입니다.
                        Util.MessageValidation("SFU4264");
                        return;
                    }
                    else
                    {
                        addRows = rowCount;
                    }
                }

                dt.Columns.Clear();
                dt.Rows.Clear();


                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                for (int i = 0; i < addRows; i++)
                {
                    DataRow dr = dt.NewRow();
                    dt.Rows.Add(dr);
                    dg.BeginEdit();
                    dg.ItemsSource = DataTableConverter.Convert(dt);
                    dg.EndEdit();
                }

                //                dg.Height = dg.ActualHeight;
                //                dg.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                //                dg.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if ((string.IsNullOrWhiteSpace(txtBoxID.Text.Trim()) && string.IsNullOrEmpty(txtCellID.Text.Trim()))
                || ((_dtOutDataSublot == null || _dtOutDataSublot.Rows.Count == 0) && string.IsNullOrEmpty(txtCellID.Text.Trim()) == false && string.IsNullOrEmpty(txtBoxID.Text.Trim())))
            {
                //SFU3413	박스ID를 스캔 또는 입력하세요.
                Util.MessageValidation("SFU3413", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtBoxID.Text = string.Empty;
                    }
                });
                return;
            }

            if (string.IsNullOrEmpty(txtBoxID.Text.Trim()) && string.IsNullOrEmpty(txtCellID.Text.Trim()) == false && _dtOutDataBox != null && _dtOutDataBox.Rows.Count > 0 && _dtOutDataSublot != null && _dtOutDataSublot.Rows.Count > 0)
            {
                ClearControl();
                _sublotId = txtCellID.Text.Trim();
                SetInBoxData(_dtOutDataBox, _dtOutDataSublot);
                GetSublotReplaceHistory(_dtOutDataBox.Rows[0]["INBOXID"].ToString(), txtCellID.Text.Trim());
                ClearInput();
            }
            else
            {
                ClearAll();
                ClearSublotInput();
                GetSublotInBox(txtBoxID.Text.Trim(), txtCellID.Text.Trim(), FocusType.Box);
                ClearInput();
            }
        }

        private void ClearInput()
        {
            txtBoxID.Text = string.Empty;
            txtCellID.Text = string.Empty;
            txtBoxID.Focus();

        }

        private void ClearSublotInput()
        {
            txtOriginSublotId.Text = string.Empty;
            txtReplaceSublotId.Text = string.Empty;
            //txtReCheckCell.Text = string.Empty;
        }

        private void ClearAll()
        {
            _dtOutDataBox = null;
            _dtOutDataSublot = null;


            ClearControl();
        }

        private void ClearControl()
        {
            txtLotID.Text = string.Empty;
            txtProdCD.Text = string.Empty;
            txtInBox.Text = string.Empty;
            txtOutBox.Text = string.Empty;
            txtPallet.Text = string.Empty;
            txtDesiccantID.Text = string.Empty;
            txtGrade.Text = string.Empty;
            txtQty.Text = string.Empty;
            txtSOC.Text = string.Empty;
            txtCellPosition.Text = string.Empty;
            dgCellHist.ItemsSource = null;
            dgCellHist.Refresh();
            dgInBox.ItemsSource = null;
            dgInBox.Columns.Clear();
            dgInBox.Refresh();

        }

        private void dgInBox_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;


                    if (_dtOutDataSublot != null && _dtOutDataSublot.Rows.Count > 0 && e.Cell.Row.Index >= 0 && e.Cell.Column.Index >= 0)
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;

                        //if (_dtOutDataSublot.Select(string.Format("ROW = {0} AND COL = {1}", e.Cell.Row.Index + 1, e.Cell.Column.Index + 1)).Length > 0)
                        if (_dtOutDataSublot.Select(string.Format("ROW = {0} AND COL = {1}", e.Cell.Row.Index + 1, e.Cell.DataGrid.Columns.Count - e.Cell.Column.Index)).Length > 0)
                        {
                            string BCOLOR = Util.NVC(GetDtRowValue(e.Cell.Row.Index, e.Cell.Column.Index, "BCOLOR"));
                            string FCOLOR = Util.NVC(GetDtRowValue(e.Cell.Row.Index, e.Cell.Column.Index, "FCOLOR"));
                            string TEXT = Util.NVC(GetDtRowValue(e.Cell.Row.Index, e.Cell.Column.Index, "TEXT"));
                            string BOLD = Util.NVC(GetDtRowValue(e.Cell.Row.Index, e.Cell.Column.Index, "BOLD"));

                            if (!string.IsNullOrEmpty(BCOLOR))
                                e.Cell.Presenter.Background = new BrushConverter().ConvertFromString(BCOLOR) as SolidColorBrush;

                            if (!string.IsNullOrEmpty(FCOLOR))
                                e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString(FCOLOR) as SolidColorBrush;

                            DataTableConverter.SetValue(e.Cell.Row.DataItem, e.Cell.Column.GetColumnText(), TEXT);

                            if (BOLD.Equals("Y"))
                            {
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                e.Cell.Presenter.BorderBrush = Brushes.Red;
                                e.Cell.Presenter.BorderThickness = new Thickness(3, 3, 3, 3);
                            }
                            else if (BOLD.Equals("N"))
                            {
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                e.Cell.Presenter.BorderBrush = Brushes.DarkGray;
                                e.Cell.Presenter.BorderThickness = new Thickness(0, 1, 0, 0);
                            }
                        }
                        else
                        {
                            //설비없음
                            e.Cell.Presenter.Background = new BrushConverter().ConvertFromString("Gray") as SolidColorBrush;
                            e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString("White") as SolidColorBrush;
                            //DataTableConverter.SetValue(e.Cell.Row.DataItem, (e.Cell.Column.Index + 1).ToString(), ObjectDic.Instance.GetObjectName("NOCELL"));
                            DataTableConverter.SetValue(e.Cell.Row.DataItem, e.Cell.Column.GetColumnText(), ObjectDic.Instance.GetObjectName("NOCELL"));
                            e.Cell.Presenter.BorderBrush = Brushes.DarkGray;
                            e.Cell.Presenter.BorderThickness = new Thickness(0, 1, 0, 0);
                        }

                        e.Cell.Presenter.Padding = new Thickness(0);
                        e.Cell.Presenter.Margin = new Thickness(0);

                    }
                }));

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private string GetDtRowValue(int row, int col, string sFindCol)
        {
            string sRtnValue = string.Empty;
            try
            {
                int columnCount = 8;
                //int columnCount = _dtOutDataSublot.AsEnumerable().Select(s => s.Field<int> ("COL")).Max().NvcInt();
                if (_dtOutDataSublot.Select(string.Format("ROW = {0} AND COL = {1}", row + 1, columnCount - col)).Length > 0)
                    return _dtOutDataSublot.Select(string.Format("ROW = {0} AND COL = {1}", row + 1, columnCount - col))[0][sFindCol].ToString();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return sRtnValue;
        }
        private enum FocusType
        {
            Box,
            Sublot,
            OriginSublot,
            ReplaceSublot
        }

        private void GetSublotInBox(String boxId, String sublotId, FocusType focusType)
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID");
                inDataTable.Columns.Add("BOXID");
                inDataTable.Columns.Add("SUBLOTID");

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["BOXID"] = boxId;
                newRow["SUBLOTID"] = sublotId;
                inDataTable.Rows.Add(newRow);

                _sublotId = sublotId;
                ShowLoadingIndicator();

                dgInBox.ItemsSource = null;

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_CELL_BOX_PSTN_MB", "INDATA", "OUTDATA_SUBLOT,OUTDATA_BOX", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }


                        DataTable dtOutDataBox = bizResult.Tables["OUTDATA_BOX"];
                        DataTable dtOutDataSublot = bizResult.Tables["OUTDATA_SUBLOT"];

                        if (dtOutDataBox.Rows.Count <= 0)
                        {
                            //조회된 값이 없습니다. 
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0232"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtBoxID.Focus();
                                }
                            });
                            return;
                        }

                        GetSublotReplaceHistory(dtOutDataBox.Rows[0]["INBOXID"].ToString(), _sublotId);
                        SetInBoxData(dtOutDataBox, dtOutDataSublot);

                        if (dtOutDataBox.Rows[0]["PACKSTAT"].ToString() == "1")  //PACKED : 1, 그외 : 0
                        {
                            btnReplaceCell.IsEnabled = false;
                            btnDesiccant.IsEnabled = false;
                            txtPackStat.Text = "Y"; //포장완료 Y

                        }
                        else
                        {
                            btnReplaceCell.IsEnabled = true;
                            btnDesiccant.IsEnabled = true;
                            txtPackStat.Text = "N"; //포장완료 N
                        }


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        switch (focusType)
                        {
                            case FocusType.Box: txtBoxID.Focus(); break;
                            case FocusType.Sublot: txtCellID.Focus(); break;
                            case FocusType.OriginSublot: txtOriginSublotId.Focus(); break;
                            case FocusType.ReplaceSublot: txtReplaceSublotId.Focus(); break;
                            default: txtBoxID.Focus(); break;
                        }

                    }

                }, indataSet);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }

        private void GetSublotReplaceHistory(string boxId, string sublotId)
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID");
                inDataTable.Columns.Add("BOXID");
                inDataTable.Columns.Add("SUBLOTID");

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["BOXID"] = boxId;
                if (string.IsNullOrEmpty(sublotId.Trim()) == false)
                    newRow["SUBLOTID"] = sublotId.Trim();
                inDataTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                dgCellHist.ItemsSource = null;

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_CELL_REPLACE_HIST_MB", "INDATA", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }


                        DataTable dtOutData = bizResult.Tables["OUTDATA"];
                        Util.GridSetData(dgCellHist, dtOutData, FrameOperation, true);


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }

                }, indataSet);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }

        private void SetInBoxData(DataTable dtOutDataBox, DataTable dtOutDataSublot)
        {
            try
            {
                // 스프레드 초기화용 변수
                #region 스프레드 초기화용 변수                
                int iRowCount;
                int iColumnCount;
                double dColumnWidth;
                double dRowHeight;
                #endregion

                // 데이터용 변수
                #region 
                bool bBold = false;
                bool bMatched = false;

                int iROW;
                int iCOL;
                string BOXID = string.Empty;
                string PKG_LOTID = string.Empty;
                string SUBLOTID = string.Empty;
                string CAPA_GRD_CODE = string.Empty;
                string FORM_TRAY_PSTN_NO = string.Empty;
                string STATUS = string.Empty;
                #endregion

                ClearControl();  //20240115

                if (dtOutDataBox != null && dtOutDataBox.Rows.Count > 0)
                {
                    txtLotID.Text = dtOutDataBox.Rows[0]["PKG_LOTID"].ToString();
                    txtProdCD.Text = dtOutDataBox.Rows[0]["PRODID"].ToString();


                    txtInBox.Text = dtOutDataBox.Rows[0]["INBOXID"].ToString();
                    txtOutBox.Text = dtOutDataBox.Rows[0]["OUTBOXID"].ToString();
                    txtPallet.Text = dtOutDataBox.Rows[0]["PLTID"].ToString();
                    txtDesiccantID.Text = dtOutDataBox.Rows[0]["DSNT_ID"].ToString();
                    txtGrade.Text = dtOutDataBox.Rows[0]["CAPA_GRD_CODE"].ToString();
                    txtQty.Text = dtOutDataBox.Rows[0]["SUBLOTQTY"].ToString();
                    txtSOC.Text = dtOutDataBox.Rows[0]["SOC_VALUE"].ToString();
                    if (string.IsNullOrEmpty(txtCellID.Text) == false && dtOutDataSublot != null
                        && dtOutDataSublot.Select(string.Format("SUBLOTID = '{0}'", txtCellID.Text.Trim())).Length > 0)
                        txtCellPosition.Text = dtOutDataSublot.Select(string.Format("SUBLOTID = '{0}'", txtCellID.Text.Trim()))[0]["FORM_TRAY_PSTN_NO"].ToString();
                }

                //열 갯수 확인
                iROW = int.Parse(dtOutDataBox.Rows[0]["ROW"].ToString());
                iCOL = int.Parse(dtOutDataBox.Rows[0]["COL"].ToString());

                List<string> rowList = new List<string>();
                for (int i = 0; i < iROW; i++)
                {
                    rowList.Add(i.ToString());
                }

                //  Row 갯수에 따라 로직을 나눔
                if (rowList.Count > 1)
                {
                    #region Grid 초기화
                    iRowCount = iROW;
                    iColumnCount = iCOL;

                    //Grid Column Width get
                    dColumnWidth = (dgInBox.ActualWidth) / (iColumnCount);

                    //Grid Row Height get
                    //double dRowHeight = Math.Round((dgFormation.ActualHeight) / (iRowCount * 2) - 1.3, 0);
                    dRowHeight = (dgInBox.ActualHeight - 40) / (iRowCount);

                    //실물이  H칼럼 부터 시작 되서 역순배치
                    DataTable dtData = new DataTable();

                    for (int i = 65 + iColumnCount - 1; i >= 65; i--)
                    {
                        SetGridHeaderSingle(Convert.ToChar(i).ToString(), dgInBox, dColumnWidth);
                        dtData.Columns.Add(Convert.ToChar(i).ToString());
                    }

                    //실물이  H칼럼 부터 시작 되서 역순배치
                    //for (int i = 0; i < dgInBox.Columns.Count; i++)
                    //{
                    //    dgInBox.Columns[i].DisplayIndex = dgInBox.Columns.Count - 1 - i;
                    //}

                    for (int i = 0; i < iROW; i++)
                    {
                        DataRow row = dtData.NewRow();
                        dtData.Rows.Add(row);
                    }

                    for (int i = 0; i < dtData.Rows.Count; i++)
                    {
                        for (int j = dtData.Columns.Count - 1, jj = 0; j >= 0; j--, jj++)
                        {
                            if (dtOutDataSublot.Select(string.Format("ROW = {0} AND COL = {1}", (i + 1), (j + 1))).Length > 0)
                            {
                                dtData.Rows[i][jj] = dtOutDataSublot.Select(string.Format("ROW = {0} AND COL = {1}", (i + 1), (j + 1)))[0]["SUBLOTID"].ToString();
                            }

                        }
                    }

                    if (dtOutDataSublot.Columns.Contains("BCOLOR") == false)
                        dtOutDataSublot.Columns.Add("BCOLOR", typeof(string));
                    if (dtOutDataSublot.Columns.Contains("FCOLOR") == false)
                        dtOutDataSublot.Columns.Add("FCOLOR", typeof(string));
                    if (dtOutDataSublot.Columns.Contains("TEXT") == false)
                        dtOutDataSublot.Columns.Add("TEXT", typeof(string));
                    if (dtOutDataSublot.Columns.Contains("BOLD") == false)
                        dtOutDataSublot.Columns.Add("BOLD", typeof(string));
                    if (dtOutDataSublot.Columns.Contains("MATCHED_YN") == false)
                        dtOutDataSublot.Columns.Add("MATCHED_YN", typeof(string));

                    for (int k = 0; k < dtOutDataSublot.Rows.Count; k++)
                    {
                        iROW = int.Parse(dtOutDataSublot.Rows[k]["ROW"].ToString());
                        iCOL = int.Parse(dtOutDataSublot.Rows[k]["COL"].ToString());

                        BOXID = Util.NVC(dtOutDataSublot.Rows[k]["OUTBOXID"].ToString());
                        PKG_LOTID = Util.NVC(dtOutDataSublot.Rows[k]["ASSY_LOT_ID"].ToString());
                        SUBLOTID = Util.NVC(dtOutDataSublot.Rows[k]["SUBLOTID"].ToString());
                        CAPA_GRD_CODE = Util.NVC(dtOutDataSublot.Rows[k]["CAPA_GRD_CODE"].ToString());
                        FORM_TRAY_PSTN_NO = Util.NVC(dtOutDataSublot.Rows[k]["FORM_TRAY_PSTN_NO"].ToString());
                        STATUS = Util.NVC(dtOutDataSublot.Rows[k]["STATUS"].ToString());

                        #region MyRegion
                        string BCOLOR = "Black";
                        string FCOLOR = "White";
                        string TEXT = string.Empty;
                        bMatched = false;
                        bBold = false;
                        #endregion

                        DataRow[] drColor = _dtColor.Select("CBO_CODE = '" + STATUS + "'");

                        if (drColor.Length > 0)
                        {
                            BCOLOR = drColor[0]["ATTRIBUTE1"].ToString();
                            FCOLOR = drColor[0]["ATTRIBUTE2"].ToString();
                        }

                        if (rdoFormPstnNo.IsChecked == true)
                        {
                            TEXT = FORM_TRAY_PSTN_NO;
                        }
                        else if (rdoGrade.IsChecked == true)
                        {
                            TEXT = CAPA_GRD_CODE;
                        }
                        else if (rdoSublotId.IsChecked == true)
                        {
                            TEXT = SUBLOTID;
                        }
                        else if (rdoLotId.IsChecked == true)
                        {
                            TEXT = PKG_LOTID;
                        }
                        else
                        {
                            TEXT = STATUS;
                        }

                        if (!string.IsNullOrEmpty(SUBLOTID) && SUBLOTID.Equals(_sublotId))
                            bBold = true;

                        dtOutDataSublot.Rows[k]["BCOLOR"] = BCOLOR;
                        dtOutDataSublot.Rows[k]["FCOLOR"] = FCOLOR;
                        dtOutDataSublot.Rows[k]["TEXT"] = TEXT;
                        dtOutDataSublot.Rows[k]["BOLD"] = (bBold == true) ? "Y" : "N";
                        dtOutDataSublot.Rows[k]["MATCHED_YN"] = (bMatched == true) ? "Y" : "N";
                        #endregion
                    }

                    _dtOutDataSublot = dtOutDataSublot;
                    _dtOutDataBox = dtOutDataBox;

                    dgInBox.RowHeight = new C1.WPF.DataGrid.DataGridLength(dRowHeight, DataGridUnitType.Pixel);
                    dgInBox.ItemsSource = DataTableConverter.Convert(dtData);
                    dgInBox.UpdateLayout();

                    ClearInput();

                    foreach (C1.WPF.DataGrid.DataGridColumn dgc in dgInBox.Columns)
                        dgc.VerticalAlignment = VerticalAlignment.Center;



                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetGridHeaderSingle(string sColName, C1DataGrid dg, double dWidth)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = sColName,
                Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.TwoWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                //CanUserResizeRows = true,
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Pixel)

            });
        }

        private void btnDesiccant_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_dtOutDataBox == null || _dtOutDataBox.Rows.Count == 0)
                {
                    //SFU3413	박스ID를 스캔 또는 입력하세요.
                    Util.MessageValidation("SFU3413", (result) =>
                    {
                        txtBoxID.Focus();
                    });
                    return;
                }

                if (_dtOutDataBox.Rows.Count > 0 && string.IsNullOrEmpty(_dtOutDataBox.Rows[0]["INBOXID"].ToString()))
                {
                    //101004	INBOX ID 가 존재하지 않습니다.
                    Util.MessageValidation("101004", (result) =>
                    {
                        txtBoxID.Focus();
                    });
                    return;
                }

                FCS002_400_REPLACE_DSNT popupReplaceDsnt = new FCS002_400_REPLACE_DSNT();
                popupReplaceDsnt.FrameOperation = FrameOperation;

                if (popupReplaceDsnt != null)
                {

                    object[] Parameters = new object[6];
                    Parameters[0] = _dtOutDataBox.Rows[0]["INBOXID"].ToString() + "A"; // INBOX는 방향코드가 필요하기 때문에 방향코드를 강제로 추가한다.
                    Parameters[1] = _dtOutDataBox.Rows[0]["DSNT_Id"].ToString();
                    C1WindowExtension.SetParameters(popupReplaceDsnt, Parameters);

                    popupReplaceDsnt.Closed += new EventHandler(popupReplaceDsnt_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.           
                    grdMain.Children.Add(popupReplaceDsnt);
                    popupReplaceDsnt.BringToFront();
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

        private void txtOriginSublotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(txtOriginSublotId.Text.Trim()))
            {
                if (_dtOutDataBox == null || _dtOutDataBox.Rows.Count == 0 || _dtOutDataSublot == null || _dtOutDataSublot.Rows.Count == 0)
                {
                    //SFU3413	박스ID를 스캔 또는 입력하세요.
                    Util.MessageValidation("SFU3413", (result) =>
                    {
                        txtBoxID.Focus();
                    });
                    return;
                }

                if (_dtOutDataSublot.Select(string.Format("SUBLOTID = '{0}'", txtOriginSublotId.Text.Trim())).Length > 0)
                {
                    _sublotId = txtOriginSublotId.Text.Trim();
                    SetInBoxData(_dtOutDataBox, _dtOutDataSublot);
                }
                else
                {
                    //SFU1681	셀의 정보를 찾을 수 없습니다.                
                    Util.MessageValidation("SFU1681", (result) =>
                    {
                        txtOriginSublotId.Focus();
                        txtOriginSublotId.SelectAll();
                    });
                    return;
                }
            }
        }

        private void txtReplaceSublotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(txtReplaceSublotId.Text.Trim()))
            {
                if (_dtOutDataBox == null || _dtOutDataBox.Rows.Count == 0 || _dtOutDataSublot == null || _dtOutDataSublot.Rows.Count == 0)
                {
                    //SFU3413	박스ID를 스캔 또는 입력하세요.
                    Util.MessageValidation("SFU3413", (result) =>
                    {
                        txtBoxID.Focus();
                    });
                    return;
                }

                if (_dtOutDataSublot.Select(string.Format("SUBLOTID = '{0}'", txtReplaceSublotId.Text.Trim())).Length > 0)
                {
                    //1123	Cell [%1]은 이미 포장되었습니다.
                    Util.MessageValidation("1123", (result) =>
                    {
                        txtReplaceSublotId.Focus();
                        txtReplaceSublotId.SelectAll();
                    }, txtReplaceSublotId.Text.Trim());
                    return;
                }
            }
        }

        private void dgInBox_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (dgInBox.Selection.SelectedRanges.Count > 0)
            {
                var firstSelectedCell = dgInBox.Selection.SelectedRanges[0].TopLeftCell;
                int firstColumnIndex = firstSelectedCell.Column.Index;
                int firstRowIndex = firstSelectedCell.Row.Index;
                string sStatus = GetDtRowValue(firstRowIndex, firstColumnIndex, "STATUS");
                string sSublotid = GetDtRowValue(firstRowIndex, firstColumnIndex, "SUBLOTID");


                //formTrayPstnNo = GetDtRowValue(dgInBox.Selection.SelectedCells[0].Row.Index, dgInBox.Selection.SelectedCells[0].Column.Index, "FORM_TRAY_PSTN_NO");
                //if (sStatus == "NOREAD")
                //{
                //    btnDelNoRead.IsEnabled = true;
                //}
                //else
                //{
                //    btnDelNoRead.IsEnabled = false;
                //}

                //if (sStatus == "NOREAD" || sStatus == "NORMAL")
                //{
                //    if (sSublotid.Length == 0)
                //    {
                //        btnReCheckCell.IsEnabled = true;
                //    }
                //    else
                //    {
                //        btnReCheckCell.IsEnabled = false;
                //    }
                //}
                //else
                //{
                //    btnReCheckCell.IsEnabled = false;
                //}

            }
        }

        //private void btnReCheckCell_Click(object sender, RoutedEventArgs e)
        //{
        //// %1 (을)를 하시겠습니까?
        //Util.MessageConfirm("SFU4329", (result) =>
        //{
        //    if (result == MessageBoxResult.OK)
        //    {

        //        //if (string.IsNullOrEmpty(txtReCheckCell.Text))
        //        //{
        //        //    //% 1을(를) 확인해 주세요.
        //        //    Util.MessageInfo("SFU8116", ObjectDic.Instance.GetObjectName("ReCheck Cell"));
        //        //    return;
        //        //}

        //        //선택된 CELL이 NOREAD 이거나 NORMAL상태인지 확인
        //        if (dgInBox.Selection.SelectedRanges.Count > 0)
        //        {
        //            var firstSelectedCell = dgInBox.Selection.SelectedRanges[0].TopLeftCell;
        //            int firstColumnIndex = firstSelectedCell.Column.Index;
        //            int firstRowIndex = firstSelectedCell.Row.Index;
        //            string sFORM_TRAY_PSTN_NO = "";
        //            string sBoxId = txtInBox.Text;

        //            string sStatus = GetDtRowValue(firstRowIndex, firstColumnIndex, "STATUS");

        //            if (sStatus == "NOREAD" || sStatus == "NORMAL")
        //            {
        //                sFORM_TRAY_PSTN_NO = GetDtRowValue(firstRowIndex, firstColumnIndex, "FORM_TRAY_PSTN_NO");

        //                ///ProcessReCheckCell(txtReCheckCell.Text);

        //            }
        //        } //END IF
        //    }
        //}, ObjectDic.Instance.GetObjectName("RECHECK_PROC"));   //RECHECK_PROC   
        //}
        #region [RECHECK LOT 화면 호출]
        private void btnReCheckCell_Click(object sender, RoutedEventArgs e)
        {
            //FCS002_400_RECHECK popup = new FCS002_400_RECHECK();
            //popup.FrameOperation = this.FrameOperation;

            //if (popup != null)
            //{
            //    object[] Parameters = new object[2];

            //    //Parameters[0] = txtWorker_Main.Tag; // 작업자id
            //    Parameters[0] = "FCS002_400"; // 자동 포장 구성(원/각형)


            //    C1WindowExtension.SetParameters(popup, Parameters);

            //    popup.Closed += new EventHandler(btnReCheckCell_Closed);
            //    grdMain.Children.Add(popup);
            //    popup.BringToFront();
            //}
        }

        private void btnReCheckCell_Closed(object sender, EventArgs e)
        {
            //FCS002_400_RECHECK popup = sender as FCS002_400_RECHECK;
            //if (popup.DialogResult == MessageBoxResult.OK)
            //{
            //    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            //}
            //this.grdMain.Children.Remove(popup);
        }
        #endregion

        private void btnDelNoRead_Click(object sender, RoutedEventArgs e)
        {
            if (_dtOutDataBox == null || _dtOutDataBox.Rows.Count == 0)
            {
                //SFU3413	박스ID를 스캔 또는 입력하세요.
                Util.MessageValidation("SFU3413", (result) =>
                {
                    txtBoxID.Focus();
                });
                return;
            }

            if (_dtOutDataBox.Rows.Count > 0 && string.IsNullOrEmpty(_dtOutDataBox.Rows[0]["INBOXID"].ToString()))
            {
                //101004	INBOX ID 가 존재하지 않습니다.
                Util.MessageValidation("101004", (result) =>
                {
                    txtBoxID.Focus();
                });
                return;
            }

            // %1 (을)를 하시겠습니까?
            //Util.MessageConfirm("SFU4329", (result) =>
            Util.MessageConfirm("SFU1230", (result) =>

            {
                if (result == MessageBoxResult.OK)
                {
                    bool bDelAll = false;
                    string sBoxId = txtInBox.Text;

                    if (chkDelAll.IsChecked == true)
                    {
                        bDelAll = true;
                    }


                    if (bDelAll) //불량셀 박스 전체 삭제시
                    {
                        if (ProcessDelNoRead("", bDelAll))
                        {
                            ClearAll();
                            ClearSublotInput();
                            GetSublotInBox(sBoxId, txtCellID.Text.Trim(), FocusType.Box);
                            ClearInput();

                        }

                    }
                    else //불량셀 개별 삭제시
                    {
                        //선택된 CELL이 NOREAD 상태인지 확인
                        if (dgInBox.Selection.SelectedRanges.Count > 0)
                        {
                            var firstSelectedCell = dgInBox.Selection.SelectedRanges[0].TopLeftCell;
                            int firstColumnIndex = firstSelectedCell.Column.Index;
                            int firstRowIndex = firstSelectedCell.Row.Index;
                            string sFORM_TRAY_PSTN_NO = "";


                            if (GetDtRowValue(firstRowIndex, firstColumnIndex, "STATUS") == "NOREAD" || GetDtRowValue(firstRowIndex, firstColumnIndex, "STATUS") == "NG")
                            {
                                sFORM_TRAY_PSTN_NO = GetDtRowValue(firstRowIndex, firstColumnIndex, "FORM_TRAY_PSTN_NO");
                                //삭제처리 BoxActHistory Note 칼럼 Update
                                if (ProcessDelNoRead(sFORM_TRAY_PSTN_NO, bDelAll))
                                {
                                    ClearAll();
                                    ClearSublotInput();
                                    GetSublotInBox(sBoxId, txtCellID.Text.Trim(), FocusType.Box);
                                    ClearInput();

                                }

                            }
                            else
                            {

                                //Util.MessageInfo("SFU3538");//선택된 데이터가 없습니다.
                                Util.MessageInfo("SFU1588");//불량항목이 없습니다.

                            }
                        }
                    }

                }
            }, ObjectDic.Instance.GetObjectName("불량셀을 삭제"));           //RECHECK_PROC   

        }

        private void popupReplaceDsnt_Closed(object sender, EventArgs e)
        {
            FCS002_400_REPLACE_DSNT popup = sender as FCS002_400_REPLACE_DSNT;

            if (_dtOutDataBox.Rows.Count > 0 && string.IsNullOrEmpty(_dtOutDataBox.Rows[0]["INBOXID"].ToString()) == false)
            {
                GetSublotInBox(_dtOutDataBox.Rows[0]["INBOXID"].ToString() + "A", string.Empty, FocusType.Box);
            }

            // 새로 조회 필요
        }

        private void btnReplaceCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_dtOutDataBox == null || _dtOutDataBox.Rows.Count == 0 || _dtOutDataSublot == null || _dtOutDataSublot.Rows.Count == 0)
                {
                    //SFU3413	박스ID를 스캔 또는 입력하세요.
                    Util.MessageValidation("SFU3413", (result) =>
                    {
                        txtBoxID.Focus();
                    });
                    return;
                }

                if (string.IsNullOrEmpty(txtOriginSublotId.Text.Trim()) && string.IsNullOrEmpty(txtReplaceSublotId.Text.Trim()))
                {
                    //SFU1462 교체 대상 Cell ID가 입력되지 않았습니다.
                    Util.MessageValidation("SFU1462", (result) =>
                    {
                        txtOriginSublotId.Focus();
                    });
                    return;
                }

                if (string.IsNullOrEmpty(txtReplaceSublotId.Text.Trim()))
                {
                    // SFU2968	취출 하시겠습니까?
                    Util.MessageConfirm("SFU2968", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            ReplaceSublot(_dtOutDataBox.Rows[0]["INBOXID"].ToString(), txtOriginSublotId.Text.Trim(), string.Empty);
                        }
                    });
                }
                else
                {
                    // SFU1465	교체처리 하시겠습니까?
                    Util.MessageConfirm("SFU1465", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            ReplaceSublot(_dtOutDataBox.Rows[0]["INBOXID"].ToString(), txtOriginSublotId.Text.Trim(), txtReplaceSublotId.Text.Trim());
                        }
                    });
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

        private void ReplaceSublot(string boxId, string originSublotId, string replaceSublotId)
        {
            string formTrayPstnNo = string.Empty;

            if (string.IsNullOrEmpty(originSublotId) == false && _dtOutDataSublot.Select(string.Format("SUBLOTID = '{0}'", originSublotId)).Length > 0)
            {
                formTrayPstnNo = _dtOutDataSublot.Select(string.Format("SUBLOTID = '{0}'", originSublotId))[0]["FORM_TRAY_PSTN_NO"].ToString();
            }
            else
            {



                if (dgInBox.Selection.SelectedRanges.Count > 0)
                {
                    var firstSelectedCell = dgInBox.Selection.SelectedRanges[0].TopLeftCell;
                    int firstColumnIndex = firstSelectedCell.Column.Index;
                    int firstRowIndex = firstSelectedCell.Row.Index;

                    formTrayPstnNo = GetDtRowValue(firstRowIndex, firstColumnIndex, "FORM_TRAY_PSTN_NO");
                }
            }



            if (string.IsNullOrEmpty(formTrayPstnNo))
            {
                //SFU1681	셀의 정보를 찾을 수 없습니다.                
                Util.MessageValidation("SFU1681", (result) =>
                {
                    txtOriginSublotId.Focus();
                    txtOriginSublotId.SelectAll();
                });
                return;
            }


            string sProcessType = "";//REPLACE, REMOVE, ADD 
            string sSource = "";
            string sTarget = "";

            if (!string.IsNullOrEmpty(originSublotId) && !string.IsNullOrEmpty(replaceSublotId)) //REPLACE, 
            {
                sProcessType = "REPLACE";
                sSource = originSublotId;
                sTarget = replaceSublotId;
            }
            else if (!string.IsNullOrEmpty(originSublotId))
            {
                sProcessType = "REMOVE";
                sTarget = originSublotId;
            }
            else if (!string.IsNullOrEmpty(replaceSublotId))
            {
                sProcessType = "ADD";
                sTarget = replaceSublotId;
            }


            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("BOXID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("PROCESS_TYPE", typeof(string));
            inDataTable.Columns.Add("SHOPID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("SRCTYPE", typeof(string));

            DataRow newRow = inDataTable.NewRow();
            newRow["BOXID"] = boxId;
            newRow["USERID"] = LoginInfo.USERID;
            newRow["PROCESS_TYPE"] = sProcessType;
            newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

            inDataTable.Rows.Add(newRow);

            DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");
            inSublotTable.Columns.Add("SUBLOTID");
            inSublotTable.Columns.Add("FORM_TRAY_PSTN_NO");
            DataRow newRow1 = inSublotTable.NewRow();
            newRow1["SUBLOTID"] = sTarget; //string.IsNullOrEmpty(originSublotId) ? replaceSublotId : originSublotId;
            //ADD시 originSublotId 이 NULL이라 replaceSublotId 가 대상이 됨
            newRow1["FORM_TRAY_PSTN_NO"] = formTrayPstnNo;

            inSublotTable.Rows.Add(newRow1);
            DataTable inSublotDelTable = indataSet.Tables.Add("INSUBLOT_DELETE");
            inSublotDelTable.Columns.Add("SUBLOTID");
            DataRow newRow2 = inSublotDelTable.NewRow();
            newRow2["SUBLOTID"] = sSource;
            inSublotDelTable.Rows.Add(newRow2);

            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SUBLOT_REPLACE_TESLA_MB", "INDATA,INSUBLOT,INSUBLOT_DELETE", null, (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    ClearInput();
                    ClearSublotInput();
                    GetSublotInBox(boxId + "A", replaceSublotId, string.IsNullOrEmpty(originSublotId) ? FocusType.ReplaceSublot : FocusType.OriginSublot);  // INBOXID는 방향코드가 들어감으로 강제로 방향코드 추가
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

        private bool ProcessDelNoRead(string pFORM_TRAY_PSTN_NO, bool AllFlag)
        {

            try
            {

                DataTable dtInfo = DataTableConverter.Convert(dgInBox.ItemsSource);

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID");
                inDataTable.Columns.Add("USERID");

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");

                inBoxTable.Columns.Add("BOXID");
                inBoxTable.Columns.Add("FORM_TRAY_PSTN_NO");

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(newRow);

                DataTable dtPalletBox = DataTableConverter.Convert(dgInBox.ItemsSource);


                int iROW = 0;
                int iCOL = 0;

                if (AllFlag == true)
                {

                    if (_dtOutDataBox != null)
                    {
                        //열 갯수 확인
                        iROW = (int)_dtOutDataBox.Rows[0]["ROW"];
                        iCOL = (int)_dtOutDataBox.Rows[0]["COL"];
                    }


                    for (int i = 0; i < iROW; i++)
                    {
                        for (int j = 0; j < iCOL; j++)
                        {
                            if (GetDtRowValue(i, j, "STATUS") == "NOREAD" || GetDtRowValue(i, j, "STATUS") == "NG")
                            {
                                newRow = inBoxTable.NewRow();
                                newRow["BOXID"] = txtInBox.Text;
                                newRow["FORM_TRAY_PSTN_NO"] = GetDtRowValue(i, j, "FORM_TRAY_PSTN_NO");
                                inBoxTable.Rows.Add(newRow);
                            }
                        }
                    }

                    if (inBoxTable.Rows.Count == 0)
                    {

                        Util.MessageInfo("SFU1588");//불량항목이 없습니다.
                        return false;

                    }

                }
                else
                {
                    newRow = inBoxTable.NewRow();
                    newRow["BOXID"] = txtInBox.Text;
                    newRow["FORM_TRAY_PSTN_NO"] = pFORM_TRAY_PSTN_NO;
                    inBoxTable.Rows.Add(newRow);
                }

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService_Multi("BR_PRD_UPD_BOXACTHISTORY_MB", "INDATA,INBOX", "", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상처리되었습니다.

                        //SetListShot(true);
                        //txtExcludeNote_SNAP.Text = string.Empty;
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
                return true;
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_UPD_BOXACTHISTORY_MB", ex.Message, ex.ToString());
                return false;
            }
        }

        private void ProcessReCheckCell(string pCell)
        {

            try
            {
                SaveScrapSubLotProcess("R");
            }
            catch (Exception ex)
            {

            }
        }
        //2024.05.31 사용안함.
        private void SaveScrapSubLotProcess(string sLotDetlTypeCode)
        {
            //    try
            //    {

            //        var firstSelectedCell = dgInBox.Selection.SelectedRanges[0].TopLeftCell;
            //        int firstColumnIndex = firstSelectedCell.Column.Index;
            //        int firstRowIndex = firstSelectedCell.Row.Index;

            //        string sStatus = GetDtRowValue(firstRowIndex, firstColumnIndex, "STATUS");

            //        DataSet inDataSet = new DataSet();
            //        DataTable inTable = inDataSet.Tables.Add("INDATA");
            //        inTable.Columns.Add("SRCTYPE", typeof(string));
            //        inTable.Columns.Add("IFMODE", typeof(string));
            //        inTable.Columns.Add("USERID", typeof(string));
            //        inTable.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string)); 
            //        inTable.Columns.Add("REMARKS_CNTT", typeof(string));
            //        inTable.Columns.Add("MENUID", typeof(string));

            //        DataRow newRow = inTable.NewRow();
            //        newRow["SRCTYPE"] = "UI";
            //        newRow["IFMODE"] = "OFF";
            //        newRow["USERID"] = LoginInfo.USERID;
            //        newRow["LOT_DETL_TYPE_CODE"] = Util.NVC(sLotDetlTypeCode);
            //        newRow["REMARKS_CNTT"] = "NoRead Cell ReCheck";
            //        newRow["MENUID"] = LoginInfo.CFG_MENUID;
            //        inTable.Rows.Add(newRow);

            //        DataTable inSubLot = inDataSet.Tables.Add("IN_SUBLOT");
            //        inSubLot.Columns.Add("SUBLOTID", typeof(string));
            //        inSubLot.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
            //        inSubLot.Columns.Add("DFCT_CODE", typeof(string));
            //        DataRow newRowSubLot = inSubLot.NewRow();

            //        string sSubLotID = "";//txtReCheckCell.Text.Trim();

            //        newRowSubLot["SUBLOTID"] = sSubLotID;
            //        newRowSubLot["DFCT_GR_TYPE_CODE"] = "E"; //EOL? TEST용으로 사용안함.
            //        newRowSubLot["DFCT_CODE"] = "INO"; //TEST용으로 사용안함 ko-KR\외관 불량 : Orhers|zh-CN\外观不良 : Orhers|en-US\Apearance Defect : Orhers|pl-PL\Apearance Defect : Orhers|uk-UA\Apearance Defect : Orhers|ru-RU\Apearance Defect : Orhers|id-ID\Apearance Defect : Orhers
            //        inSubLot.Rows.Add(newRowSubLot);

            //        ShowLoadingIndicator();

            //        new ClientProxy().ExecuteService_Multi("BR_SET_SUBLOT_TRANSFER_LOT_DETL_TYPE_MB", "INDATA,IN_SUBLOT", "OUTDATA", (bizResult, bizException) =>
            //        {
            //            try
            //            {
            //                if (bizException != null)
            //                {
            //                    HiddenLoadingIndicator();
            //                    Util.MessageException(bizException);
            //                    return;
            //                }


            //                HiddenLoadingIndicator();
            //                Util.MessageInfo("SFU1275");//정상처리되었습니다.

            //                if (sStatus == "NOREAD")  //NOREAD상태이면 RECHECK 처리 이후 NOREAD 삭제 처리할지 확인한다.
            //                {
            //                    btnDelNoRead_Click(null, null);
            //                }

            //            }
            //            catch (Exception ex)
            //            {
            //                HiddenLoadingIndicator();
            //                Util.MessageException(ex);
            //            }
            //        }, inDataSet, menuid: FrameOperation.MENUID);
            //    }
            //    catch (Exception ex)
            //    {
            //        HiddenLoadingIndicator();
            //        Util.MessageException(ex);
            //    }
        }

        #endregion
    }
}
