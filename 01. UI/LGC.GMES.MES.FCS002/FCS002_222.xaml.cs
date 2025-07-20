/*************************************************************************************
 Created Date : 2023.10.19
      Creator : 최성필
   Decription : 재공정보현황(Model별)
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.16  DEVELOPER : Initial Created.
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
using System.Windows.Threading;
using System.Linq; //20220119_모델별 합계 Row 추가

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_222 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS002_222()
        {
            InitializeComponent();
        }
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();

            dtpFromDate.SelectedDateTime = DateTime.Today.AddMonths(-3);
            dtpToDate.SelectedDateTime = DateTime.Today;

            this.Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelChild = { cboRoute };
            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINEMODEL", cbChild: cboModelChild, cbParent: cboModelParent);

            C1ComboBox nCbo = new C1ComboBox();
            C1ComboBox[] cboRouteParent = { cboLine, cboModel, nCbo, nCbo };
            _combo.SetCombo(cboRoute, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "ROUTE", cbParent: cboRouteParent);

            string[] sFilter1 = { "COMBO_FORM_SPCL_FLAG" };
            _combo.SetCombo(cboSpecial, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);
            
        }
        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();

                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("SPCL_FLAG", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                if (!string.IsNullOrEmpty(txtLotId.Text)) dr["PROD_LOTID"] = txtLotId.Text;
                dr["SPCL_FLAG"] = Util.GetCondition(cboSpecial, bAllNull: true);
              //  dr["FROM_DATE"] = GetConvertDateValue(dtpFromDate);
              //  dr["TO_DATE"] = GetConvertDateValue(dtpToDate);
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd") ;
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd");
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_WIP_RETRIEVE_INFO_MODEL_MB", "RQSTDT", "RSLTDT", dtRqst);

                DataTable NewTable = new DataTable();
                if (dtRslt.Rows.Count > 0)
                {

                    Util _Util = new Util();
                    string[] sColumnName = new string[] { "MDLLOT_ID", "PRODID", "PROD_LOTID" };
                    _Util.SetDataGridMergeExtensionCol(dgWipbyModel, sColumnName, DataGridMergeMode.VERTICAL);
                }
                Util.GridSetData(dgWipbyModel, dtRslt, FrameOperation, true);
                //AddRowSumnPerQty(dgWipbyModel);

            }


            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void AddRowSumnPerQty(C1.WPF.DataGrid.C1DataGrid datagrid)
        {
            DataTable dtTemp = ConvertToDataTable(datagrid);

            int iTrayCnt = dtTemp.Columns["TRAYCNT"].Ordinal;
            int iInputCol = dtTemp.Columns["INPUT"].Ordinal;
            int iWipCol = dtTemp.Columns["WIP_QTY"].Ordinal;
            int iSumFromCol = dtTemp.Columns["A"].Ordinal;
            int iSumToCol = dtTemp.Columns["Z"].Ordinal;
            int iSumInputQty = 0;
            int iTotalSumQty = 0;
            int iSumQty = 0;
            double ColValue = 0;
            string ColName = string.Empty;

            if (dtTemp.Rows.Count > 0)
            {
                DataRow row_sum = dtTemp.NewRow();
                row_sum["PROD_LOTID"] = ObjectDic.Instance.GetObjectName("합계");
                for (int iCol = 0; iCol < dtTemp.Columns.Count; iCol++)
                {
                    ColName = dtTemp.Columns[iCol].ColumnName;

                    if (iCol.Equals(iTrayCnt) || iCol.Equals(iInputCol) || iCol.Equals(iWipCol) || (iCol >= iSumFromCol && iCol <= iSumToCol))
                    {
                        for (int iRow = 0; iRow < dtTemp.Rows.Count; iRow++)
                        {
                            iSumQty = Convert.ToInt32(Util.NVC(dtTemp.Rows[iRow][ColName]));
                            iTotalSumQty = iTotalSumQty + iSumQty;
                        }

                        row_sum[ColName] = Convert.ToString(iTotalSumQty);

                        if (iCol.Equals(iInputCol) || iCol.Equals(iTrayCnt))
                        {
                            iSumInputQty = iTotalSumQty;
                        }
                    }
                    iTotalSumQty = 0;
                    iSumQty = 0;
                }

                DataRow row_pre = dtTemp.NewRow();
                row_pre["PROD_LOTID"] = ObjectDic.Instance.GetObjectName("PERCENT_VAL");
                for (int iCol = 0; iCol < dtTemp.Columns.Count; iCol++)
                {
                    ColName = dtTemp.Columns[iCol].ColumnName;

                    if (iCol.Equals(iTrayCnt) || iCol.Equals(iWipCol) || (iCol >= iSumFromCol && iCol <= iSumToCol))
                    {
                        for (int iRow = 0; iRow < dtTemp.Rows.Count; iRow++)
                        {
                            iSumQty = Convert.ToInt32(dtTemp.Rows[iRow][ColName]);
                            iTotalSumQty = iTotalSumQty + iSumQty;
                        }

                        if (ColName.Equals("TRAYCNT"))
                            continue;
                        else
                            ColValue = Convert.ToDouble(iTotalSumQty) / Convert.ToDouble(iSumInputQty) * 100;

                        if (ColValue == 0)
                        {
                            row_pre[ColName] = "0.00";
                        }
                        else
                        {
                            row_pre[ColName] = ColValue.ToString("#,#.00");
                        }
                    }
                    iTotalSumQty = 0;
                    iSumQty = 0;
                }

                dtTemp.Rows.Add(row_sum);
                dtTemp.Rows.Add(row_pre);

                Util.GridSetData(datagrid, dtTemp, FrameOperation, true);
            }
        }

        public static DataTable ConvertToDataTable(C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();
                if (dg.ItemsSource == null)
                {
                    foreach (C1.WPF.DataGrid.DataGridColumn column in dg.Columns)
                    {
                        if (!string.IsNullOrEmpty(column.Name))
                            dt.Columns.Add(column.Name);
                    }
                    return dt;
                }
                else
                {
                    dt = ((DataView)dg.ItemsSource).Table;
                    return dt;
                }
            }
            catch (Exception ex)
            {
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgWipbyModel_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgWipbyModel.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null) return;

                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (string.IsNullOrEmpty(e.Cell.Column.Name) == false)
                    {                        
                        if (cboSpecial.SelectedValue.ToString() == "Y")
                        {
                            if (e.Cell.Column.Name.Equals("SPCL_TYPE_CODE") || e.Cell.Column.Name.Equals("SPCL_NOTE_CNTT"))
                            {
                                if ((!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name.ToString())).ToString().Equals("0")))
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_FLAG"))) &&
                            !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_FLAG")).ToString().Equals("N"))
                        {
                            if (e.Cell.Column.Name.Equals("SPCL_TYPE_CODE") || e.Cell.Column.Name.Equals("SPCL_NOTE_CNTT") ||
                                e.Cell.Column.Name.Equals("FORM_SPCL_REL_SCHD_DTTM") || e.Cell.Column.Name.Equals("TRAYCNT") ||
                                e.Cell.Column.Name.Equals("INPUT") || e.Cell.Column.Name.Equals("WIP_QTY") ||
                                e.Cell.Column.Name.Equals("A") || e.Cell.Column.Name.Equals("B") ||
                                e.Cell.Column.Name.Equals("C") || e.Cell.Column.Name.Equals("D") ||
                                e.Cell.Column.Name.Equals("E") || e.Cell.Column.Name.Equals("F") ||
                                e.Cell.Column.Name.Equals("G") || e.Cell.Column.Name.Equals("H") ||
                                e.Cell.Column.Name.Equals("I") || e.Cell.Column.Name.Equals("J") ||
                                e.Cell.Column.Name.Equals("K") || e.Cell.Column.Name.Equals("L") ||
                                e.Cell.Column.Name.Equals("M") || e.Cell.Column.Name.Equals("N") ||
                                e.Cell.Column.Name.Equals("O") || e.Cell.Column.Name.Equals("P") ||
                                e.Cell.Column.Name.Equals("Q") || e.Cell.Column.Name.Equals("R") ||
                                e.Cell.Column.Name.Equals("S") || e.Cell.Column.Name.Equals("T") ||
                                e.Cell.Column.Name.Equals("U") || e.Cell.Column.Name.Equals("V") ||
                                e.Cell.Column.Name.Equals("W") || e.Cell.Column.Name.Equals("X") ||
                                e.Cell.Column.Name.Equals("Y") || e.Cell.Column.Name.Equals("Z") || e.Cell.Column.Name.Equals("GRD_1") )
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }

                        if (e.Cell.Column.Name.Equals("PROD_LOTID"))
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_LOTID")).ToString().Equals(ObjectDic.Instance.GetObjectName("합계")))
                            {
                                e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.PapayaWhip);
                                dgWipbyModel.GetCell(e.Cell.Row.Index, (e.Cell.Column.Index - 1)).Presenter.Visibility = Visibility.Collapsed;
                            }
                            else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_LOTID")).ToString().Equals(ObjectDic.Instance.GetObjectName("PERCENT_VAL")))
                            {
                                e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);
                                dgWipbyModel.GetCell(e.Cell.Row.Index, (e.Cell.Column.Index - 1)).Presenter.Visibility = Visibility.Collapsed;
                            }
                        }
                        //if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_LOTID")).ToString().Equals(Util.NVC(ObjectDic.Instance.GetObjectName("합계"))))
                        //{
                        //    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);
                        //}
                        //else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_LOTID")).ToString().Equals(Util.NVC(ObjectDic.Instance.GetObjectName("ALL_SUM"))))
                        //{
                        //    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                        //}
                    }
                }));
        }

        private void dgWipbyModel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (cell.Row.Index < 2 || cell.Column.Index.Equals(0)) return;

                if (dg.CurrentColumn.Index < dgWipbyModel.Columns["PROD_LOTID"].Index) return;
                if (dg.CurrentCell.Text.Equals("0")) return;
                if (dg.CurrentRow.Index == dg.Rows.Count - 1) return;

                int rowIdx = dg.CurrentRow.Index;

                object[] Parameters = new object[19];
                Parameters[0] = null; //sOPER
                Parameters[1] = null; //sOPER_NAME
                Parameters[2] = Util.NVC(Util.GetCondition(cboLine, bAllNull: true)); // sLINE_ID
                Parameters[3] = Util.NVC(cboLine.Text); //sLINE_NAME
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "ROUTID")); //sLotID
                Parameters[5] = Util.NVC(cboRoute.Text); //sROUTE_NAME
                Parameters[6] = Util.NVC(Util.GetCondition(cboModel, bAllNull: true)); //sMODEL_ID
                Parameters[7] = Util.NVC(cboModel.Text); //sMODEL_NAME
                Parameters[8] = "A"; //sStatus
                Parameters[9] = null; //sStatusNameB
                Parameters[10] = null; // Util.NVC(Util.GetCondition(cboRouteDG, bAllNull: true)); //_sROUTE_TYPE_DG
                Parameters[11] = null; // Util.NVC(cboRouteDG.Text); //sRouteTypeDGName
                Parameters[12] = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "PROD_LOTID")); //sLotID
                //Parameters[13] = Util.NVC(Util.GetCondition(cboSpecial, bAllNull: true)); // sSpecial

                string sSpecial = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "SPCL_FLAG"));
                if (!string.IsNullOrEmpty(sSpecial))
                {
                    sSpecial = sSpecial.Equals("N") ? "N" : "Y";
                }
                Parameters[13] = Util.NVC(sSpecial); // sSpecial
                // Parameters[14] = Util.NVC(cboSpecial.Text); // sSpecialName
                Parameters[14] = null;
                Parameters[15] = null;
                Parameters[16] = null;
                //Parameters[17] = Util.NVC(Util.GetCondition(cboLotType, bAllNull: true));
                //Parameters[18] = Util.NVC(cboLotType.Text);
                               
                this.FrameOperation.OpenMenuFORM("FCS002_005_02", "FCS002_005_02", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("Tray List"), true, Parameters);
            }
        }

        private void dgWipbyModel_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetList();
            }
        }

        private void dgWipbyModel_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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
               

                if (!Util.NVC(dgWipbyModel.GetCell(e.Row.Index, dgWipbyModel.Columns["PROD_LOTID"].Index).Value).Equals(Util.NVC(ObjectDic.Instance.GetObjectName("합계")))
                    && !Util.NVC(dgWipbyModel.GetCell(e.Row.Index, dgWipbyModel.Columns["PROD_LOTID"].Index).Value).Equals(Util.NVC(ObjectDic.Instance.GetObjectName("ALL_SUM"))))
                {
                    tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    e.Row.HeaderPresenter.Content = tb;
                }
            }
        }


        #endregion

        #region 20-1. DateTimePicker Convert Date2Lot [Year]
        private static string GetConvertDateYear(int parmYear)
        {
            string[] arrAlphabet = new string[20] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T" };
            string lsYears = "";
            int liMode = 20;
            int liYears = Convert.ToInt16(parmYear) - 2000;
            //int intCnt1 = (int)((liYears - 1) / liMode);
            int intCnt2 = (int)((liYears - 1) % liMode);

            lsYears = arrAlphabet[intCnt2];

            //if (intCnt1 > 0)
            //{
            //    lsYears = arrAlphabet[1 + intCnt2];
            //}
            //else
            //{
            //    lsYears = arrAlphabet[liYears];
            //}

            return lsYears;
        }
        #endregion

        #region 20-2. DateTimePicker Convert Date2Lot [Month]
        private static string GetConvertDateMonth(int parmMonth)
        {
            string[] arrAlphabet = new string[27] { null, "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            string lsMonths = "";

            if (Convert.ToInt16(parmMonth) >= 1 && Convert.ToInt16(parmMonth) <= 12)
            {
                lsMonths = arrAlphabet[parmMonth];
            }
            return lsMonths;
        }
        #endregion

        #region 20-3. DateTimePicker Convert Date2Lot
        private static string GetConvertDateValue(LGCDatePicker pDTP)
        {
            string lsDateValueLOT = string.Empty;
            string lsFROM_DATE = string.Empty;
            string lsTO_DATE = string.Empty;

            lsDateValueLOT = GetConvertDateYear(Convert.ToInt16(pDTP.SelectedDateTime.ToString("yyyy"))) +
                             GetConvertDateMonth(Convert.ToInt16(pDTP.SelectedDateTime.ToString("MM"))) +
                             pDTP.SelectedDateTime.ToString("dd");
            if (String.IsNullOrEmpty(lsDateValueLOT))
            {
                return null;
            }

            return lsDateValueLOT;
        }
        #endregion
    }
}
