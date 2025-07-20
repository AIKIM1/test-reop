/*************************************************************************************
 Created Date : 2021.10.05
      Creator : 이대근
   Decription : RTLS 현황판
--------------------------------------------------------------------------------------
 [Change History]
  2021.10.05  DEVELOPER : Initial Created.
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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_137 : UserControl, IWorkArea
    {
        Util _Util = new Util();
        string[] aLineStat = new string[20];



        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public COM001_137()
        {
            InitializeComponent();
            InitCombo();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboLineParent = { cboArea };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboLineParent);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                GetLineCellStatus();

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void GetLineCellStatus()
        {
            try
            {
                string sBizName = string.Empty;
                sBizName = "DA_RTLS_GET_STOCK_CELL_UI";

                Util.gridClear(dgCell);

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Trim() == "" ? null : Util.GetCondition(cboEquipmentSegment);

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(sBizName, "INDATA", "RSLTDT", dtRqst, (dtRsltCell, ex) =>
                {
                    if (ex != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    dgCell.ItemsSource = DataTableConverter.Convert(dtRsltCell);

                    if (dtRsltCell.Rows.Count > 0)
                    {
                        string[] sColumnName = new string[] { "LV1", "LV2", "LV3" };
                        _Util.SetDataGridMergeExtensionCol(dgCell, sColumnName, DataGridMergeMode.HORIZONTAL);

                        for (int i = 0; i < dtRsltCell.Rows.Count; i++)
                        {
                            if (Util.NVC(dtRsltCell.Rows[i]["ROWNUM"]).Equals("1"))
                            {
                                aLineStat[0] = (Util.NVC(dtRsltCell.Rows[i]["QTY1"]).ToString());
                            }

                            if (Util.NVC(dtRsltCell.Rows[i]["ROWNUM"]).Equals("2"))
                            {
                                aLineStat[1] = (Util.NVC(dtRsltCell.Rows[i]["QTY1"]).ToString());
                            }

                            if (Util.NVC(dtRsltCell.Rows[i]["ROWNUM"]).Equals("5"))
                            {
                                aLineStat[2] = (Util.NVC(dtRsltCell.Rows[i]["QTY1"]).ToString());
                            }

                            if (Util.NVC(dtRsltCell.Rows[i]["ROWNUM"]).Equals("6"))
                            {
                                aLineStat[3] = (Util.NVC(dtRsltCell.Rows[i]["QTY1"]).ToString());
                            }

                            if (Util.NVC(dtRsltCell.Rows[i]["ROWNUM"]).Equals("11"))
                            {
                                aLineStat[4] = (Util.NVC(dtRsltCell.Rows[i]["QTY1"]).ToString());
                            }
                            if (Util.NVC(dtRsltCell.Rows[i]["ROWNUM"]).Equals("15"))
                            {
                                aLineStat[5] = (Util.NVC(dtRsltCell.Rows[i]["QTY1"]).ToString());
                            }
                            if (Util.NVC(dtRsltCell.Rows[i]["ROWNUM"]).Equals("16"))
                            {
                                aLineStat[6] = (Util.NVC(dtRsltCell.Rows[i]["QTY1"]).ToString());
                            }

                        }
                    }

                    GetLineCMAStatus();
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        private void GetLineCMAStatus()
        {
            try
            {
                string sBizName = string.Empty;
                sBizName = "DA_RTLS_GET_STOCK_CMA_UI";

                Util.gridClear(dgModule);

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Trim() == "" ? null : Util.GetCondition(cboEquipmentSegment);

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(sBizName, "INDATA", "RSLTDT", dtRqst, (dtRsltCMA, ex) =>
                {
                    if (ex != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    dgModule.ItemsSource = DataTableConverter.Convert(dtRsltCMA);

                    if (dtRsltCMA.Rows.Count > 0)
                    {
                        string[] sColumnName = new string[] { "LV1", "LV2", "LV3" };
                        _Util.SetDataGridMergeExtensionCol(dgModule, sColumnName, DataGridMergeMode.HORIZONTAL);

                        for (int i = 0; i < dtRsltCMA.Rows.Count; i++)
                        {
                            if (Util.NVC(dtRsltCMA.Rows[i]["ROWNUM"]).Equals("1"))
                            {
                                aLineStat[7] = (Util.NVC(dtRsltCMA.Rows[i]["QTY1"]).ToString());
                            }

                            if (Util.NVC(dtRsltCMA.Rows[i]["ROWNUM"]).Equals("2"))
                            {
                                aLineStat[8] = (Util.NVC(dtRsltCMA.Rows[i]["QTY1"]).ToString());
                            }

                            if (Util.NVC(dtRsltCMA.Rows[i]["ROWNUM"]).Equals("3"))
                            {
                                aLineStat[9] = (Util.NVC(dtRsltCMA.Rows[i]["QTY1"]).ToString());
                            }

                            if (Util.NVC(dtRsltCMA.Rows[i]["ROWNUM"]).Equals("10"))
                            {
                                aLineStat[10] = (Util.NVC(dtRsltCMA.Rows[i]["QTY1"]).ToString());
                            }

                            if (Util.NVC(dtRsltCMA.Rows[i]["ROWNUM"]).Equals("14"))
                            {
                                aLineStat[11] = (Util.NVC(dtRsltCMA.Rows[i]["QTY1"]).ToString());
                            }
                            if (Util.NVC(dtRsltCMA.Rows[i]["ROWNUM"]).Equals("16"))
                            {
                                aLineStat[12] = (Util.NVC(dtRsltCMA.Rows[i]["QTY1"]).ToString());
                            }

                        }
                    }

                    GetLineBMAStatus();
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void GetLineBMAStatus()
        {
            try
            {
                string sBizName = string.Empty;
                sBizName = "DA_RTLS_GET_STOCK_BMA_UI";

                Util.gridClear(dgPack);

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Trim() == "" ? null : Util.GetCondition(cboEquipmentSegment);

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(sBizName, "INDATA", "RSLTDT", dtRqst, (dtRsltBMA, ex) =>
                {
                    if (ex != null)
                    {
                        ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    dgPack.ItemsSource = DataTableConverter.Convert(dtRsltBMA);

                    if (dtRsltBMA.Rows.Count > 0)
                    {
                        string[] sColumnName = new string[] { "LV1", "LV2", "LV3" };
                        _Util.SetDataGridMergeExtensionCol(dgPack, sColumnName, DataGridMergeMode.HORIZONTAL);

                        for (int i = 0; i < dtRsltBMA.Rows.Count; i++)
                        {
                            if (Util.NVC(dtRsltBMA.Rows[i]["ROWNUM"]).Equals("1"))
                            {
                                aLineStat[13] = (Util.NVC(dtRsltBMA.Rows[i]["QTY1"]).ToString());
                            }

                            if (Util.NVC(dtRsltBMA.Rows[i]["ROWNUM"]).Equals("2"))
                            {
                                aLineStat[14] = (Util.NVC(dtRsltBMA.Rows[i]["QTY1"]).ToString());
                            }

                            if (Util.NVC(dtRsltBMA.Rows[i]["ROWNUM"]).Equals("3"))
                            {
                                aLineStat[15] = (Util.NVC(dtRsltBMA.Rows[i]["QTY1"]).ToString());
                            }

                            if (Util.NVC(dtRsltBMA.Rows[i]["ROWNUM"]).Equals("6"))
                            {
                                aLineStat[16] = (Util.NVC(dtRsltBMA.Rows[i]["QTY1"]).ToString());
                            }
                            if (Util.NVC(dtRsltBMA.Rows[i]["ROWNUM"]).Equals("11"))
                            {
                                aLineStat[17] = (Util.NVC(dtRsltBMA.Rows[i]["QTY1"]).ToString());
                            }
                            if (Util.NVC(dtRsltBMA.Rows[i]["ROWNUM"]).Equals("12"))
                            {
                                aLineStat[18] = (Util.NVC(dtRsltBMA.Rows[i]["QTY1"]).ToString());
                            }
                        }
                    }

                    SetLineStatus();
                });
                
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void SetLineStatus()
        {
            try
            {
                DataTable dtSum = new DataTable();
                dtSum.Columns.Add("C1", typeof(string));
                dtSum.Columns.Add("C2", typeof(string));
                dtSum.Columns.Add("C3", typeof(string));
                dtSum.Columns.Add("C4", typeof(string));
                dtSum.Columns.Add("C5", typeof(string));
                dtSum.Columns.Add("C6", typeof(string));
                dtSum.Columns.Add("C7", typeof(string));
                dtSum.Columns.Add("M1", typeof(string));
                dtSum.Columns.Add("M2", typeof(string));
                dtSum.Columns.Add("M3", typeof(string));
                dtSum.Columns.Add("M4", typeof(string));
                dtSum.Columns.Add("M5", typeof(string));
                dtSum.Columns.Add("M6", typeof(string));
                dtSum.Columns.Add("P1", typeof(string));
                dtSum.Columns.Add("P2", typeof(string));
                dtSum.Columns.Add("P3", typeof(string));
                dtSum.Columns.Add("P4", typeof(string));
                dtSum.Columns.Add("P5", typeof(string));
                dtSum.Columns.Add("P6", typeof(string));

                DataRow ds = dtSum.NewRow();

                ds["C1"] = aLineStat[0];
                ds["C2"] = aLineStat[1];
                ds["C3"] = aLineStat[2];
                ds["C4"] = aLineStat[3];
                ds["C5"] = aLineStat[4];
                ds["C6"] = aLineStat[5];
                ds["C7"] = aLineStat[6];
                ds["M1"] = aLineStat[7];
                ds["M2"] = aLineStat[8];
                ds["M3"] = aLineStat[9];
                ds["M4"] = aLineStat[10];
                ds["M5"] = aLineStat[11];
                ds["M6"] = aLineStat[12];
                ds["P1"] = aLineStat[13];
                ds["P2"] = aLineStat[14];
                ds["P3"] = aLineStat[15];
                ds["P4"] = aLineStat[16];
                ds["P5"] = aLineStat[17];
                ds["P6"] = aLineStat[18];

                dtSum.Rows.Add(ds);

                Util.GridSetData(dgPalletInfo, dtSum, FrameOperation);
                dtSum.Clear();

                loadingIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void dgCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.C1DataGrid c1Gd = (C1.WPF.DataGrid.C1DataGrid)sender;
            C1.WPF.DataGrid.DataGridCell cell = c1Gd.GetCellFromPoint(pnt);
            C1.WPF.DataGrid.DataGridCell cell2 = dgCell.GetCellFromPoint(pnt);

            string sRownum = string.Empty;
            sRownum = (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[cell.Row.Index].DataItem, "ROWNUM")));

            //EQSSGID,ROWNUM
            loadingIndicator.Visibility = Visibility.Visible;
            string strEqsgId = Util.GetCondition(cboEquipmentSegment).Trim() == "" ? null : Util.GetCondition(cboEquipmentSegment);
            string[] sParam = { strEqsgId, sRownum , "CELL" };
            this.FrameOperation.OpenMenu("SFU10999510", true, sParam);
            loadingIndicator.Visibility = Visibility.Collapsed;

        }

        private void dgModule_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.C1DataGrid c1Gd = (C1.WPF.DataGrid.C1DataGrid)sender;
            C1.WPF.DataGrid.DataGridCell cell = c1Gd.GetCellFromPoint(pnt);
            C1.WPF.DataGrid.DataGridCell cell2 = dgModule.GetCellFromPoint(pnt);

            string sRownum = string.Empty;
            sRownum = (Util.NVC(DataTableConverter.GetValue(dgModule.Rows[cell.Row.Index].DataItem, "ROWNUM")));

            //EQSSGID,ROWNUM
            loadingIndicator.Visibility = Visibility.Visible;
            string strEqsgId = Util.GetCondition(cboEquipmentSegment).Trim() == "" ? null : Util.GetCondition(cboEquipmentSegment);
            string[] sParam = { strEqsgId, sRownum , "CMA" };
            this.FrameOperation.OpenMenu("SFU10999510", true, sParam);
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void dgPack_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.C1DataGrid c1Gd = (C1.WPF.DataGrid.C1DataGrid)sender;
            C1.WPF.DataGrid.DataGridCell cell = c1Gd.GetCellFromPoint(pnt);
            C1.WPF.DataGrid.DataGridCell cell2 = dgPack.GetCellFromPoint(pnt);

            string sRownum = string.Empty;
            sRownum = (Util.NVC(DataTableConverter.GetValue(dgPack.Rows[cell.Row.Index].DataItem, "ROWNUM")));

            //EQSSGID,ROWNUM
            loadingIndicator.Visibility = Visibility.Visible;
            string strEqsgId = Util.GetCondition(cboEquipmentSegment).Trim() == "" ? null : Util.GetCondition(cboEquipmentSegment);
            string[] sParam = { strEqsgId, sRownum, "BMA" };
            this.FrameOperation.OpenMenu("SFU10999510", true, sParam);
            loadingIndicator.Visibility = Visibility.Collapsed;
        }
    }
}
