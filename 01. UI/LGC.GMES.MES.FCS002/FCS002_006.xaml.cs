/*************************************************************************************
 Created Date : 2023.01.16
      Creator : 강동희
   Decription : 재공정보현황(Lot별)
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
    public partial class FCS002_006 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS002_006()
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

            string[] sFilter1 = { "FORM_SPCL_FLAG_MCC" };
            _combo.SetCombo(cboSpecial, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            // Lot 유형
            _combo.SetCombo(cboLotType, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LOTTYPE"); // 2021.08.19 Lot 유형 검색조건 추가
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
                dtRqst.Columns.Add("SPCL_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("BY_MODEL", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["ROUTID"] = Util.GetCondition(cboRoute, bAllNull: true);
                if (!string.IsNullOrEmpty(txtLotId.Text)) dr["PROD_LOTID"] = txtLotId.Text;
                dr["SPCL_TYPE_CODE"] = Util.GetCondition(cboSpecial, bAllNull: true);
                dr["BY_MODEL"] = (bool)chkModel.IsChecked ? "Y" : "N";
                dr["LOTTYPE"] = Util.GetCondition(cboLotType, bAllNull: true);

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_WIP_RETRIEVE_INFO_SPECIAL_LOTID_INDUSTRY_MODEL_MB", "RQSTDT", "RSLTDT", dtRqst);
                if (chkModel.IsChecked == true)
                {
                    DataTable NewTable = new DataTable();
                    if (dtRslt.Rows.Count > 0)
                    {
                        NewTable = gridSumRowAddByModel(dtRslt);
                        NewTable.Merge(gridSumRowAddALL(dtRslt));

                        Util _Util = new Util();
                        string[] sColumnName = new string[] { "MDLLOT_ID", "MODEL_NAME", "PROD_LOTID" };
                        _Util.SetDataGridMergeExtensionCol(dgWipbyLot, sColumnName, DataGridMergeMode.VERTICAL);
                    }
                    Util.GridSetData(dgWipbyLot, NewTable, FrameOperation, true);
                }
                else
                {
                    DataTable NewTable = new DataTable();
                    if (dtRslt.Rows.Count > 0)
                    {
                        NewTable = dtRslt.Copy();
                        NewTable.Merge(gridSumRowAddALL(dtRslt));

                        Util _Util = new Util();
                        string[] sColumnName = new string[] { "PROD_LOTID" };
                        _Util.SetDataGridMergeExtensionCol(dgWipbyLot, sColumnName, DataGridMergeMode.VERTICAL);
                    }
                    Util.GridSetData(dgWipbyLot, NewTable, FrameOperation, true);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable gridSumRowAddByModel(DataTable dtRslt)
        {
            DataTable NewTable = new DataTable();
            DataTable TmpTable = new DataTable();

            var modelList = dtRslt.AsEnumerable()
            .GroupBy(g => new
            {
                MODEL_ID = g.Field<string>("MDLLOT_ID"),
                MODEL_NAME = g.Field<string>("MODEL_NAME")
            })
            .Select(f => new
            {
                modelID = f.Key.MODEL_ID,
                modelName = f.Key.MODEL_NAME
            })
            .OrderBy(o => o.modelID).ToList();

            foreach (var modelItem in modelList)
            {
                TmpTable = dtRslt.Select("MDLLOT_ID = '" + Util.NVC(modelItem.modelID) + "'").CopyToDataTable();

                int WaitTray = 0;
                int WaitCell = 0;
                int WorkTray = 0;
                int WorkCell = 0;
                int AgingEndTray = 0;
                int AgingEndCell = 0;
                int TroubleTray = 0;
                int TroubleCell = 0;
                int RecheckTray = 0;
                int RecheckCell = 0;
                int TotalTray = 0;
                int TotalInputCell = 0;
                int TotalCurrCell = 0;

                for (int iRow = 0; iRow < TmpTable.Rows.Count; iRow++)
                {
                    WaitTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["WAITTRAY"])));
                    WaitCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["WAITCELL"])));
                    WorkTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["WORKTRAY"])));
                    WorkCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["WORKCELL"])));
                    AgingEndTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGENDTRAY"])));
                    AgingEndCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGENDCELL"])));
                    TroubleTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TROUBLETRAY"])));
                    TroubleCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TROUBLECELL"])));
                    RecheckTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["RECHECKTRAY"])));
                    RecheckCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["RECHECKCELL"])));
                    TotalTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TOTALTRAY"])));
                    TotalInputCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TOTALINPUTCELL"])));
                    TotalCurrCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TOTALCURRCELL"])));
                }

                DataRow newRow = TmpTable.NewRow();
                newRow["MDLLOT_ID"] = Util.NVC(modelItem.modelID);
                newRow["MODEL_NAME"] = Util.NVC(modelItem.modelName);
                newRow["PROD_LOTID"] = ObjectDic.Instance.GetObjectName("합계");
                newRow["WAITTRAY"] = WaitTray;
                newRow["WAITCELL"] = WaitCell;
                newRow["WORKTRAY"] = WorkTray;
                newRow["WORKCELL"] = WorkCell;
                newRow["AGINGENDTRAY"] = AgingEndTray;
                newRow["AGINGENDCELL"] = AgingEndCell;
                newRow["TROUBLETRAY"] = TroubleTray;
                newRow["TROUBLECELL"] = TroubleCell;
                newRow["RECHECKTRAY"] = RecheckTray;
                newRow["RECHECKCELL"] = RecheckCell;
                newRow["TOTALTRAY"] = TotalTray;
                newRow["TOTALINPUTCELL"] = TotalInputCell;
                newRow["TOTALCURRCELL"] = TotalCurrCell;
                TmpTable.Rows.Add(newRow);

                NewTable.Merge(TmpTable);
            }
            dtRslt = NewTable.Copy();

            return dtRslt;
        }

        private DataTable gridSumRowAddALL(DataTable dtRslt)
        {
            DataTable NewTable = new DataTable();
            DataTable TmpTable = new DataTable();

            var typeList = dtRslt.AsEnumerable()
            .GroupBy(g => new
            {
                SPCL_TYPE = g.Field<string>("SPCL_TYPE_CODE"),
                SPCLL_NAME = g.Field<string>("SPECIAL_NAME")
            })
            .Select(f => new
            {
                spclType = f.Key.SPCL_TYPE,
                spclName = f.Key.SPCLL_NAME
            })
            .OrderBy(o => o.spclType).ToList();

            NewTable = dtRslt.Copy();
            NewTable.Clear();

            foreach (var typeItem in typeList)
            {
                TmpTable = dtRslt.Select("SPCL_TYPE_CODE = '" + Util.NVC(typeItem.spclType) + "'").CopyToDataTable();

                int WaitTray = 0;
                int WaitCell = 0;
                int WorkTray = 0;
                int WorkCell = 0;
                int AgingEndTray = 0;
                int AgingEndCell = 0;
                int TroubleTray = 0;
                int TroubleCell = 0;
                int RecheckTray = 0;
                int RecheckCell = 0;
                int TotalTray = 0;
                int TotalInputCell = 0;
                int TotalCurrCell = 0;

                for (int iRow = 0; iRow < TmpTable.Rows.Count; iRow++)
                {
                    WaitTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["WAITTRAY"])));
                    WaitCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["WAITCELL"])));
                    WorkTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["WORKTRAY"])));
                    WorkCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["WORKCELL"])));
                    AgingEndTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGENDTRAY"])));
                    AgingEndCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["AGINGENDCELL"])));
                    TroubleTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TROUBLETRAY"])));
                    TroubleCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TROUBLECELL"])));
                    RecheckTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["RECHECKTRAY"])));
                    RecheckCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["RECHECKCELL"])));
                    TotalTray += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TOTALTRAY"])));
                    TotalInputCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TOTALINPUTCELL"])));
                    TotalCurrCell += Convert.ToInt32(Util.NVC(Convert.ToString(TmpTable.Rows[iRow]["TOTALCURRCELL"])));
                }

                DataRow newRow = NewTable.NewRow();
                newRow["MDLLOT_ID"] = string.Empty;
                newRow["MODEL_NAME"] = string.Empty;
                newRow["PROD_LOTID"] = ObjectDic.Instance.GetObjectName("ALL_SUM");
                newRow["SPCL_TYPE_CODE"] = Util.NVC(typeItem.spclType);
                newRow["SPECIAL_NAME"] = Util.NVC(typeItem.spclName);
                newRow["WAITTRAY"] = WaitTray;
                newRow["WAITCELL"] = WaitCell;
                newRow["WORKTRAY"] = WorkTray;
                newRow["WORKCELL"] = WorkCell;
                newRow["AGINGENDTRAY"] = AgingEndTray;
                newRow["AGINGENDCELL"] = AgingEndCell;
                newRow["TROUBLETRAY"] = TroubleTray;
                newRow["TROUBLECELL"] = TroubleCell;
                newRow["RECHECKTRAY"] = RecheckTray;
                newRow["RECHECKCELL"] = RecheckCell;
                newRow["TOTALTRAY"] = TotalTray;
                newRow["TOTALINPUTCELL"] = TotalInputCell;
                newRow["TOTALCURRCELL"] = TotalCurrCell;
                NewTable.Rows.Add(newRow);
            }
            dtRslt = NewTable.Copy();

            return dtRslt;
        }
        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void dgWipbyLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgWipbyLot.Dispatcher.BeginInvoke(new Action(() =>
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
                        if (e.Cell.Column.Name.Equals("WORKTRAY") || e.Cell.Column.Name.Equals("WORKCELL") || e.Cell.Column.Name.Equals("AGINGENDTRAY") || e.Cell.Column.Name.Equals("AGINGENDCELL") ||
                            e.Cell.Column.Name.Equals("WAITCELL") || e.Cell.Column.Name.Equals("WAITTRAY") || e.Cell.Column.Name.Equals("RECHECKTRAY") || e.Cell.Column.Name.Equals("RECHECKCELL") ||
                            e.Cell.Column.Name.Equals("TOTALTRAY") || e.Cell.Column.Name.Equals("TOTALINPUTCELL"))
                        {
                            if ((!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name.ToString())).ToString().Equals("0")))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }

                        if (cboSpecial.SelectedValue.ToString() == "Y")
                        {
                            if (e.Cell.Column.Name.Equals("WORKTRAY") || e.Cell.Column.Name.Equals("WORKCELL") || e.Cell.Column.Name.Equals("AGINGENDTRAY") || e.Cell.Column.Name.Equals("AGINGENDCELL") ||
                                e.Cell.Column.Name.Equals("WAITCELL") || e.Cell.Column.Name.Equals("WAITTRAY") || e.Cell.Column.Name.Equals("TOTALTRAY") || e.Cell.Column.Name.Equals("TOTALINPUTCELL"))
                            {
                                if ((!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name.ToString())).ToString().Equals("0")))
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_TYPE_CODE"))) &&
                            !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_TYPE_CODE")).ToString().Equals("N"))
                        {
                            if (!e.Cell.Column.Name.Equals("MDLLOT_ID") && !e.Cell.Column.Name.Equals("MODEL_NAME") && !e.Cell.Column.Name.Equals("PROD_LOTID"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }

                        if (e.Cell.Column.Name.Equals("TROUBLETRAY") || e.Cell.Column.Name.Equals("TROUBLECELL"))
                        {
                            if ((!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name.ToString())).ToString().Equals("0")))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.DarkViolet);
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                        }

                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_LOTID")).ToString().Equals(Util.NVC(ObjectDic.Instance.GetObjectName("합계"))))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);
                        }
                        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_LOTID")).ToString().Equals(Util.NVC(ObjectDic.Instance.GetObjectName("ALL_SUM"))))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                        }
                    }
                }));
        }

        private void dgWipbyLot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

            if (cell != null)
            {
                if (cell.Row.Index < 2 || cell.Column.Index.Equals(0)) return;

                if (dg.CurrentColumn.Index < dgWipbyLot.Columns["WAITTRAY"].Index) return;
                if (dg.CurrentCell.Text.Equals("0")) return;
                if (dg.CurrentRow.Index == dg.Rows.Count - 1) return;

                int rowIdx = dg.CurrentRow.Index;

                object[] Parameters = new object[19];
                Parameters[0] = null; //sOPER
                Parameters[1] = null; //sOPER_NAME
                Parameters[2] = Util.NVC(Util.GetCondition(cboLine, bAllNull: true)); // sLINE_ID
                Parameters[3] = Util.NVC(cboLine.Text); //sLINE_NAME
                Parameters[4] = Util.NVC(Util.GetCondition(cboRoute, bAllNull: true));//sROUTE_ID
                Parameters[5] = Util.NVC(cboRoute.Text); //sROUTE_NAME
                Parameters[6] = Util.NVC(Util.GetCondition(cboModel, bAllNull: true)); //sMODEL_ID
                Parameters[7] = Util.NVC(cboModel.Text); //sMODEL_NAME
                Parameters[8] = null; //sStatus
                Parameters[9] = null; //sStatusNameB
                Parameters[10] = null; // Util.NVC(Util.GetCondition(cboRouteDG, bAllNull: true)); //_sROUTE_TYPE_DG
                Parameters[11] = null; // Util.NVC(cboRouteDG.Text); //sRouteTypeDGName
                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "PROD_LOTID")) == "전체합계")
                    Parameters[12] = null;
                else
                    Parameters[12] = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "PROD_LOTID")); //sLotID
                //Parameters[13] = Util.NVC(Util.GetCondition(cboSpecial, bAllNull: true)); // sSpecial

                string sSpecial = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "SPCL_TYPE_CODE"));
                if (!string.IsNullOrEmpty(sSpecial))
                {
                    sSpecial = sSpecial.Equals("N") ? "N" : "Y";
                }
                Parameters[13] = Util.NVC(sSpecial); // sSpecial
                // Parameters[14] = Util.NVC(cboSpecial.Text); // sSpecialName
                Parameters[14] = null;
                Parameters[15] = null;
                Parameters[16] = null;
                Parameters[17] = Util.NVC(Util.GetCondition(cboLotType, bAllNull: true));
                Parameters[18] = Util.NVC(cboLotType.Text);

                if (dg.CurrentColumn.Name.Equals("WORKTRAY") || dg.CurrentColumn.Name.Equals("WORKCELL"))
                {
                    Parameters[8] = "S"; // sStatus
                    Parameters[9] = ObjectDic.Instance.GetObjectName("WORK_TRAY"); // 작업 Tray
                }

                else if (dg.CurrentColumn.Name.Equals("AGINGENDTRAY") || dg.CurrentColumn.Name.Equals("AGINGENDCELL"))
                {
                    Parameters[8] = "P"; // sStatus
                    Parameters[9] = ObjectDic.Instance.GetObjectName("AGING_END_WAIT"); // Aging 종료대기
                }
                else if (dg.CurrentColumn.Name.Equals("WAITTRAY") || dg.CurrentColumn.Name.Equals("WAITCELL"))
                {
                    Parameters[8] = "E"; // sStatus
                    Parameters[9] = ObjectDic.Instance.GetObjectName("AFTER_END_WAIT"); // 작업종료 후 대기
                }
                else if (dg.CurrentColumn.Name.Equals("TROUBLETRAY") || dg.CurrentColumn.Name.Equals("TROUBLECELL"))
                {
                    Parameters[8] = "T"; // sStatus
                    Parameters[9] = ObjectDic.Instance.GetObjectName("WORK_ERR"); // 작업이상
                }
                else if (dg.CurrentColumn.Name.Equals("RECHECKTRAY") || dg.CurrentColumn.Name.Equals("RECHECKCELL"))
                {
                    Parameters[8] = "R"; // sStatus
                    Parameters[9] = ObjectDic.Instance.GetObjectName("RECHECK"); // RECHECK
                }
                else if (dg.CurrentColumn.Name.Equals("TOTALTRAY") || dg.CurrentColumn.Name.Equals("TOTALINPUTCELL"))
                {
                    Parameters[8] = "A"; // sStatus
                    Parameters[9] = ObjectDic.Instance.GetObjectName("TOTAL"); // 
                }

                this.FrameOperation.OpenMenuFORM("FCS002_005_02", "FCS002_005_02", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("Tray List"), true, Parameters);
            }
        }

        private void dgWipbyLot_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgWipbyLot_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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
                if (!Util.NVC(dgWipbyLot.GetCell(e.Row.Index, dgWipbyLot.Columns["PROD_LOTID"].Index).Value).Equals(Util.NVC(ObjectDic.Instance.GetObjectName("합계")))
                    && !Util.NVC(dgWipbyLot.GetCell(e.Row.Index, dgWipbyLot.Columns["PROD_LOTID"].Index).Value).Equals(Util.NVC(ObjectDic.Instance.GetObjectName("ALL_SUM"))))
                {
                    tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    e.Row.HeaderPresenter.Content = tb;
                }
            }
        }

        private void chkModel_Checked(object sender, RoutedEventArgs e)
        {
            dgWipbyLot.Columns["MDLLOT_ID"].Visibility = Visibility.Visible;
            dgWipbyLot.Columns["MODEL_NAME"].Visibility = Visibility.Visible;
            GetList();
        }

        private void chkModel_Unchecked(object sender, RoutedEventArgs e)
        {
            dgWipbyLot.Columns["MDLLOT_ID"].Visibility = Visibility.Collapsed;
            dgWipbyLot.Columns["MODEL_NAME"].Visibility = Visibility.Collapsed;
            GetList();
        }


        #endregion

    }
}
