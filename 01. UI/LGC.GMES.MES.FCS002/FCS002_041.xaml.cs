/*************************************************************************************
 Created Date : 2023.01.25
      Creator : 강동희
   Decription : 출하 예정일 조회
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.25  NAME : Initial Created
  2023.01.02 주훈    Grid Columm를 xaml에서 코드에서 구현
  2024.01.11 주훈    Dynamic Query로 변경
  2024.03.05 주훈    출하 예정일 이상 조회 항목 추가에 따른 수정
  2024.03.07 주훈  : 합계 구현 방법 변경
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
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using System.Windows.Data;
using static LGC.GMES.MES.CMM001.Controls.UcBaseDataGrid;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// TSK_710.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_041 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]

        // 2024.03.05 추가
        // 고정 Column 갯수
        private int FIXED_COULUMN_COUNT = 2;

        #endregion

        #region [Initialize]
        public FCS002_041()
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
            //Combo Setting
            InitCombo();

            // 2023.01.02 추가
            InitSpread();

            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();

            //공정경로 별 조회
            C1ComboBox[] cboLineChild = { cboModel };
            ComCombo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            ComCombo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox[] cboRouteParent = { cboLine, cboModel };
            ComCombo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent);

        }

        private void InitSpread()
        {
            Util.gridClear(dgShipPlanDate); // Grid clear
            int Header_Row_count = 2;

            //칼럼 헤더 행 추가
            for (int i = 0; i < Header_Row_count; i++)
            {
                DataGridColumnHeaderRow HR = new DataGridColumnHeaderRow();
                dgShipPlanDate.TopRows.Add(HR);
            }

            // FIX
            FixedMultiHeader("SHIP_PLAN_DATE|SHIP_PLAN_DATE", "SHIPING_DATE", iWidth: 100, oHorizonAlign: HorizontalAlignment.Center, sSumType:"합계");
            FixedMultiHeader("AFTER_YN|AFTER_YN", "AFTER_YN", iWidth: 100, bVisible:false, sSumType: null);  // 2024.03.05 출하일 이상 조회 추가

            // 이전 Biz Column 대응
            FixedMultiHeader("NORMAL_AGING|A3101", "A3101", sTag: "FF3101");
            FixedMultiHeader("NORMAL_AGING|A3102", "A3102", sTag: "FF3102");
            FixedMultiHeader("NORMAL_AGING|A3103", "A3103", sTag: "FF3103");
            FixedMultiHeader("NORMAL_AGING|A3104", "A3104", sTag: "FF3104");
            FixedMultiHeader("NORMAL_AGING|A3105", "A3105", sTag: "FF3105");

            FixedMultiHeader("HIGH_AGING|A4101", "A4101", sTag: "FF4101");
            FixedMultiHeader("HIGH_AGING|A4102", "A4102", sTag: "FF4102");
            FixedMultiHeader("HIGH_AGING|A4103", "A4103", sTag: "FF4103");
            FixedMultiHeader("HIGH_AGING|A4104", "A4104", sTag: "FF4104");
            FixedMultiHeader("HIGH_AGING|A4105", "A4105", sTag: "FF4105");
        }


        private void FixedMultiHeader(string sName, string sBindName
                                            , int iWidth = 75, bool bPercent = false, bool bVisible = true
                                            , HorizontalAlignment oHorizonAlign = HorizontalAlignment.Right
                                            , VerticalAlignment oVerticalAlign = VerticalAlignment.Center
                                            , string sSumType = "SUM"
                                            , string sTag = null
                                    )
        {
            bool bReadOnly = true;
            bool bEditable = false;

            string[] sColNames = sName.Split('|');

            List<string> Multi_Header = new List<string>();
            Multi_Header = sColNames.ToList();

            var column_TEXT = CreateTextColumn(null, Multi_Header, sBindName, sBindName, iWidth
                                                , bReadOnly: bReadOnly, bEditable: bEditable, bVisible: bVisible, bPercent: bPercent
                                                , oHorizonAlign: oHorizonAlign, oVerticalAlign: oVerticalAlign, sSumType: sSumType
                                                , sTag: sTag);
            dgShipPlanDate.Columns.Add(column_TEXT);
        }

        private C1.WPF.DataGrid.DataGridTextColumn CreateTextColumn(string Single_Header, List<string> Multi_Header, string sName, string sBinding, int iWidth
                                                                    , bool bReadOnly = false
                                                                    , bool bEditable = true
                                                                    , bool bVisible = true
                                                                    , bool bPercent = false
                                                                    , HorizontalAlignment oHorizonAlign = HorizontalAlignment.Right
                                                                    , VerticalAlignment oVerticalAlign = VerticalAlignment.Center
                                                                    , string sSumType = "SUM"
                                                                    , string sTag = null
                                                        )
        {

            C1.WPF.DataGrid.DataGridTextColumn Col = new C1.WPF.DataGrid.DataGridTextColumn();

            Col.Name = sName;
            Col.Binding = new Binding(sBinding);
            Col.IsReadOnly = bReadOnly;
            Col.EditOnSelection = bEditable;
            Col.Visibility = bVisible.Equals(true) ? Visibility.Visible : Visibility.Collapsed;
            Col.HorizontalAlignment = oHorizonAlign;
            Col.VerticalAlignment = oVerticalAlign;

            if (String.IsNullOrEmpty(sTag) == false)
                Col.Tag = sTag;

            if (iWidth == 0)
                Col.Width = new C1.WPF.DataGrid.DataGridLength(1, DataGridUnitType.Auto);
            else
                Col.Width = new C1.WPF.DataGrid.DataGridLength(iWidth, DataGridUnitType.Pixel);

            if (bPercent)
                Col.Format = "P2";

            if (!string.IsNullOrEmpty(Single_Header))
                Col.Header = Single_Header;
            else
                Col.Header = Multi_Header;

            // 2024.03.07 Footer Sum 추가
            switch (sSumType)
            {
                case "합계":
                    DataGridAggregate.SetAggregateFunctions(Col, new DataGridAggregatesCollection { new DataGridAggregateText("합계") { ResultTemplate = grdMain.Resources["ResultTemplateSum"] as DataTemplate } });
                    break;
                case "SUM":
                    DataGridAggregate.SetAggregateFunctions(Col, new DataGridAggregatesCollection { new DataGridAggregateSum { ResultTemplate = grdMain.Resources["ResultTemplate"] as DataTemplate } });
                    break;
                case "EVEN":
                    DataGridAggregate.SetAggregateFunctions(Col, new DataGridAggregatesCollection { new DataGridAggregateEven { ResultTemplate = grdMain.Resources["ResultTemplate"] as DataTemplate } });
                    break;
                default:
                    break;
            }

            return Col;
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

        #region [Method]

        private void ClearFilterGrid()
        {
            try
            {
                //dgShipPlanDate.ClearFilter(); 동기화로 변경
                if ((dgShipPlanDate.ItemsSource != null) && (dgShipPlanDate.Columns.Count > 0))
                {
                    dgShipPlanDate.FilterBy(dgShipPlanDate.Columns[0], null);
                }
            }
            finally { }
        }

        private void GetList()
        {
            try
            {
                btnSearch.IsEnabled = false;
                Util.gridClear(dgShipPlanDate);

                int iColCount = dgShipPlanDate.Columns.Count;

                // 2024.03.05 Column 추가에 따른 수정
                for (int i = iColCount - 1; i > (FIXED_COULUMN_COUNT - 1); i--)
                {
                    if (dgShipPlanDate.Columns[i].Name.Substring(0, 1) != "A")
                        break;

                    dgShipPlanDate.Columns.RemoveAt(i);
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                if (!string.IsNullOrEmpty(txtLotId.Text)) dr["PROD_LOTID"] = Util.NVC(txtLotId.Text);
                dtRqst.Rows.Add(dr);

                // 2023.01.02 수정
                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SHIPMENT_DATE_MB", "RQSTDT", "RSLTDT", dtRqst);
                //Util.GridSetData(dgShipPlanDate, dtRslt, this.FrameOperation, true);
                //////0인 컬럼 숨기기 Start
                //if (dgShipPlanDate.Rows.Count > 1)
                //{
                //    for (int i = 1; i < dgShipPlanDate.Columns.Count; i++)
                //    {
                //        if (dgShipPlanDate[dgShipPlanDate.Rows.Count - 1, i].Text == "0")
                //        {
                //            dgShipPlanDate.Columns[i].Visibility = Visibility.Collapsed;
                //        }
                //    }
                //}

                ShowLoadingIndicator();
                // 2024.01.11 Dynamic Query로 변경
                //string sBizName = "DA_SEL_SHIPMENT_DATE_MB";
                string sBizName = "DA_SEL_SHIPMENT_DATE_MB";
                new ClientProxy().ExecuteService(sBizName, "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        DataTable dtRslt = result.Copy();
                        AddColmunShipPlanDate(dtRslt);

                        // 2024.03.07 DataGridSummaryRow 구현으로 삭제
                        //AddRowSumQty(dtRslt);

                        Util.GridSetData(dgShipPlanDate, dtRslt, this.FrameOperation, true);
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
                Util.MessageException(ex);
            }
            finally
            {
                btnSearch.IsEnabled = true;
            }
        }

        /// <summary>
        /// Add Colmun ShipPlanDate
        /// </summary>
        private void AddColmunShipPlanDate(DataTable dt)
        {
            int iCol = 0;
            string sColName = string.Empty;
            String sFixHeader1 = String.Empty;
            string sHeaderPref = String.Empty;
            string sheaderTail = String.Empty;
            string sTagPref = String.Empty;
            string sTagData = String.Empty;

            try
            {
                // Example
                //FixedMultiHeader("NORMAL_AGING|A3101", "A3101", sTag: "FF3101");
                //FixedMultiHeader("HIGH_AGING|A4101", "A4101", sTag: "FF4101");

                // A31 : 상온
                sFixHeader1 = "NORMAL_AGING";
                sHeaderPref = "A31";
                sTagPref = "FF31";
                for (iCol = 0; iCol < dt.Columns.Count; iCol++)
                {
                    sColName = dt.Columns[iCol].ColumnName;

                    if (sColName.Contains(sHeaderPref) == false)
                        continue;

                    if (dgShipPlanDate.Columns.Contains(sColName) == true)
                        continue;

                    sheaderTail = sColName.Substring(sHeaderPref.Length);
                    sTagData = sTagPref + String.Format("{0:00}", Convert.ToInt32(sheaderTail));

                    FixedMultiHeader(sFixHeader1 + "|" + sColName, sColName, sTag: sTagData);
                }

                // A41 : 고온
                sFixHeader1 = "HIGH_AGING";
                sHeaderPref = "A41";
                sTagPref = "FF41";
                for (iCol = 0; iCol < dt.Columns.Count; iCol++)
                {
                    sColName = dt.Columns[iCol].ColumnName;

                    if (sColName.Contains(sHeaderPref) == false)
                        continue;

                    if (dgShipPlanDate.Columns.Contains(sColName) == true)
                        continue;

                    sheaderTail = sColName.Substring(sHeaderPref.Length);
                    sTagData = sTagPref + String.Format("{0:00}", Convert.ToInt32(sheaderTail));

                    FixedMultiHeader(sFixHeader1 + "|" + sColName, sColName, sTag: sTagData);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Add Row SumQty
        /// </summary>
        private void AddRowSumQty(DataTable dt)
        {
            int iRow = 0;
            int iCol = 0;
            int iStaCol = 0;

            string ColName = string.Empty;

            // 2024.03.05 출하일 이상 조회(AFTER_YN) 추가
            List<String> ExceptColName = new List<string>
            {
                "SHIPING_DATE", "AFTER_YN"
            };

            int iMaxRow = 0;
            DataRow dr = dt.NewRow();

            dr["SHIPING_DATE"] = ObjectDic.Instance.GetObjectName("합계");

            iMaxRow = dt.Rows.Count;
            iStaCol = dt.Columns.IndexOf("SHIPING_DATE");
            for (iRow = 0; iRow < iMaxRow; iRow++)
            {
                for (iCol = iStaCol; iCol < dt.Columns.Count; iCol++)
                {
                    ColName = dt.Columns[iCol].ColumnName;
                    if (ExceptColName.Contains(ColName) == true)
                        continue;

                    dr[iCol] = Util.NVC_Int(dr[iCol]) + Util.NVC_Int(dt.Rows[iRow][iCol]);
                }
            }
            dt.Rows.Add(dr);
        }


        #endregion

        #region [Event]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearFilterGrid(); // 2024.03.13 추가
            GetList();
        }

        private void dgShipPlanDate_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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

                if (cell.Text == datagrid.CurrentColumn.Header.ToString()) return;
                if (cell.Column.Name == "SHIPING_DATE") return;
                if (cell.Value.Equals("0")) return;

                // 2024.03.05 함계 항목도 조회 가능하도록 수정
                //if (cell.Row.Index == datagrid.Rows.Count - 1) return;

                FCS002_042 FCS002_042 = new FCS002_042();
                FCS002_042.FrameOperation = FrameOperation;

                object[] Parameters = new object[10];
                Parameters[0] = Util.NVC(txtLotId.Text);        // PROD_LOTID
                Parameters[1] = Util.GetCondition(cboLine);     // EQSGID
                Parameters[2] = Util.GetCondition(cboModel);    // MDLLOT_ID
                Parameters[3] = Util.GetCondition(cboRoute);    // ROUTID
                Parameters[4] = Util.NVC(cell.Column.Tag);      // PROCID

                // 2024.03.05 함계 항목도 조회 가능하도록 수정
                if (cell.Row.Index < datagrid.Rows.Count - 1)
                {
                    Parameters[5] = Util.NVC(DataTableConverter.GetValue(dgShipPlanDate.CurrentRow.DataItem, "SHIPING_DATE")); //SHIPING_DATE
                }

                Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgShipPlanDate.CurrentRow.DataItem, "AFTER_YN")); // 2024.03.05 추가

                this.FrameOperation.OpenMenuFORM("FCS002_042", "FCS002_042", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("출하 예정 Tray List"), true, Parameters);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgShipPlanDate_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    // 2023.01.02 수정
                    //if (Util.NVC(e.Cell.Column.Name.Substring(0, 1)).Equals("A") &&
                    //    !Util.NVC(e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Text).Equals("0"))
                    //{
                    //    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    //}

                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (Util.NVC(e.Cell.Column.Name).Equals("SHIPING_DATE") == true)
                    {
                        if (Util.NVC(e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Text).Equals(ObjectDic.Instance.GetObjectName("합계")) == true)
                        {
                            e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.PapayaWhip);
                        }
                        else
                        {
                            e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                        }
                    }

                    if ((e.Cell.Column.Index >= FIXED_COULUMN_COUNT) &&
                        (Util.NVC(e.Cell.Column.Name.Substring(0, 1)).Equals("A") == true) &&
                        (Util.NVC(e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Text).Equals("0") == false))
                    {
                        e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.DataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }));
        }

        private void dgShipPlanDate_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgShipPlanDate_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index - (dataGrid.TopRows.Count - 1) > 0 && e.Row.Index < dataGrid.Rows.Count - dataGrid.BottomRows.Count)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        #endregion
    }
}
