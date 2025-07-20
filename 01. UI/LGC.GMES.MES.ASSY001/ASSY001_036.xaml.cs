/*************************************************************************************
 Created Date : 
      Creator :
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]

 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_036 : UserControl, IWorkArea
    {

        CommonCombo combo = new CommonCombo();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_036()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;
            ApplyPermissions();

            initcombo();
            SetGrid();

        }
        private void cboEquipmentSegment_SelectedValueChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {

        }

        private void txtLOTID_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchData();

            }
        }

        private void btnSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender == null)
                return;

            SearchData();
        }

        #region[Method]

        private void initcombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            //C1ComboBox[] cboLineChild = { cboElecType };
            C1ComboBox[] cboLineParent = { cboArea };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbParent: cboLineParent);
        }

        private void SetGrid()
        {
            #region Lamination
            DataTable dtLami = new DataTable();
            dtLami.Columns.Add("PROCESS", typeof(string));
            dtLami.Columns.Add("TYPE", typeof(string));
            dtLami.Columns.Add("QTY", typeof(double));
            dtLami.Columns.Add("CSTQTY", typeof(double));

            DataRow dLRow = null;
            dLRow = dtLami.NewRow();
            dLRow["PROCESS"] = ObjectDic.Instance.GetObjectName("매거진");
            dLRow["TYPE"] = "A-Type";
            dLRow["QTY"] = 0;
            dLRow["CSTQTY"] = 0;

            dtLami.Rows.Add(dLRow);

            dLRow = dtLami.NewRow();
            dLRow["PROCESS"] = ObjectDic.Instance.GetObjectName("매거진");
            dLRow["TYPE"] = "C-Type";
            dLRow["QTY"] = 0;
            dLRow["CSTQTY"] = 0;
            dtLami.Rows.Add(dLRow);

            Util.GridSetData(dgLami, dtLami, FrameOperation);
            #endregion

            #region Notching
            DataTable dtNoth = new DataTable();
            dtNoth.Columns.Add("PROCESS", typeof(string));
            dtNoth.Columns.Add("TYPE", typeof(string));
            dtNoth.Columns.Add("QTY", typeof(double));
            dtNoth.Columns.Add("ROLLQTY", typeof(double));

            DataRow dNRow = null;
            dNRow = dtNoth.NewRow();
            dNRow["PROCESS"] = ObjectDic.Instance.GetObjectName("전극");
            dNRow["TYPE"] = ObjectDic.Instance.GetObjectName("필요양극");
            dNRow["QTY"] = 0;
            dNRow["ROLLQTY"] = 0;
            dtNoth.Rows.Add(dNRow);

            dNRow = dtNoth.NewRow();
            dNRow["PROCESS"] = ObjectDic.Instance.GetObjectName("전극");
            dNRow["TYPE"] = ObjectDic.Instance.GetObjectName("필요음극");
            dNRow["QTY"] = 0;
            dNRow["ROLLQTY"] = 0;
            dtNoth.Rows.Add(dNRow);

            Util.GridSetData(dgNotching, dtNoth, FrameOperation);
            #endregion

            #region 기준정보
            DataTable dtBase = new DataTable();
            dtBase.Columns.Add("TITLE", typeof(string));
            dtBase.Columns.Add("TYPE", typeof(string));
            dtBase.Columns.Add("QTY", typeof(double));

            DataRow dBRow = null;
            dBRow = dtBase.NewRow();
            dBRow["TITLE"] = ObjectDic.Instance.GetObjectName("PKG Cell 당 Bi-cell 수량");
            dBRow["TYPE"] = "";
            dBRow["QTY"] = 0;
            dtBase.Rows.Add(dBRow);

            dBRow = dtBase.NewRow();
            dBRow["TITLE"] = ObjectDic.Instance.GetObjectName("Roll당 타수");
            dBRow["TYPE"] = "A-Type";
            dBRow["QTY"] = 0;
            dtBase.Rows.Add(dBRow);

            dBRow = dtBase.NewRow();
            dBRow["TITLE"] = ObjectDic.Instance.GetObjectName("Roll당 타수");
            dBRow["TYPE"] = "C-Type";
            dBRow["QTY"] = 0;
            dtBase.Rows.Add(dBRow);

            dBRow = dtBase.NewRow();
            dBRow["TITLE"] = ObjectDic.Instance.GetObjectName("MZ당 Bi-Cell 수량");
            dBRow["TYPE"] = "A-Type";
            dBRow["QTY"] = 0;
            dtBase.Rows.Add(dBRow);

            dBRow = dtBase.NewRow();
            dBRow["TITLE"] = ObjectDic.Instance.GetObjectName("MZ당 Bi-Cell 수량");
            dBRow["TYPE"] = "C-Type";
            dBRow["QTY"] = 0;
            dtBase.Rows.Add(dBRow);

            dBRow = dtBase.NewRow();
            dBRow["TITLE"] = ObjectDic.Instance.GetObjectName("PKG Cell 당 Bi-cell");
            dBRow["TYPE"] = "A-Type";
            dBRow["QTY"] = 0;
            dtBase.Rows.Add(dBRow);

            dBRow = dtBase.NewRow();
            dBRow["TITLE"] = ObjectDic.Instance.GetObjectName("PKG Cell 당 Bi-cell");
            dBRow["TYPE"] = "C-Type";
            dBRow["QTY"] = 0;
            dtBase.Rows.Add(dBRow);

            dBRow = dtBase.NewRow();
            dBRow["TITLE"] = ObjectDic.Instance.GetObjectName("PKG Cell 당 전극");
            dBRow["TYPE"] = ObjectDic.Instance.GetObjectName("양극");
            dBRow["QTY"] = 0;
            dtBase.Rows.Add(dBRow);

            dBRow = dtBase.NewRow();
            dBRow["TITLE"] = ObjectDic.Instance.GetObjectName("PKG Cell 당 전극");
            dBRow["TYPE"] = ObjectDic.Instance.GetObjectName("음극");
            dBRow["QTY"] = 0;
            dtBase.Rows.Add(dBRow);

            Util.GridSetData(dgBase, dtBase, FrameOperation);
            
            #endregion
        }

        private void dgBase_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
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
                        if (e.Cell.Column.Name.ToUpper().IndexOf("QTY") >= 0)
                        {
                            C1.WPF.DataGrid.DataGridCellPresenter p = dataGrid.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter;
                            StackPanel panel = p.Content as StackPanel;

                            for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                            {
                                int ValueToIndex = e.Cell.Row.Index;

                                if (ValueToIndex < 5)
                                {
                                    panel.Children[0].Visibility = Visibility.Visible;
                                    panel.Children[1].Visibility = Visibility.Collapsed;                                     
                                }                                    

                                else
                                {
                                    panel.Children[0].Visibility = Visibility.Collapsed;
                                    panel.Children[1].Visibility = Visibility.Visible;
                                }                                    
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SearchData()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("STRT_DTTM", typeof(string));
                dt.Columns.Add("END_DTTM", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Process.VD_LMN;
             

                //if (txtLOTID.Text.Equals(""))
                //{
                //    dr["EQSGID"] = Convert.ToString(cboVDEquipmentSegment.SelectedValue);
                //    dr["STRT_DTTM"] = Util.NVC(ldpDateFrom.SelectedDateTime.ToShortDateString());
                //    dr["END_DTTM"] = Util.NVC(ldpDateTo.SelectedDateTime.ToShortDateString());
                //    dr["EQPTID"] = Convert.ToString(cboVDEquipment.SelectedValue).Equals("") ? null : Convert.ToString(cboVDEquipment.SelectedValue);
                //    dr["LOTID"] = null;
                //}
                //else
                //{

                //    dr["EQSGID"] = null;
                //    dr["STRT_DTTM"] = null;
                //    dr["END_DTTM"] = null;
                //    dr["EQPTID"] = null;
                //    dr["LOTID"] = txtLOTID.Text;
                //}
                
                dt.Rows.Add(dr);


                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIPHISTORY_QA_JUDG_VALUE", "RQST","RSLT", dt, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.GridSetData(dgLotInfo, bizResult, FrameOperation, true);
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        #endregion

        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            CMM_ASSY_PLAN_CELL wndPlan = new CMM_ASSY_PLAN_CELL();

            wndPlan.FrameOperation = FrameOperation;

            if (wndPlan != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = Util.GetCondition(cboEquipmentSegment);
                Parameters[1] = Util.GetCondition(cboEquipmentSegment);
                Parameters[2] = Util.GetCondition(cboEquipmentSegment);
                Parameters[3] = Util.GetCondition(cboEquipmentSegment);
                C1WindowExtension.SetParameters(wndPlan, Parameters);

                wndPlan.Closed += new EventHandler(wndPlan_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndWaitLot.ShowModal()));
                grdMain.Children.Add(wndPlan);
                wndPlan.BringToFront();
            }
        }

        private void wndPlan_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_PLAN_CELL window = sender as CMM_ASSY_PLAN_CELL;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            this.grdMain.Children.Remove(window);
        }
        private void txtQTY_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                decimal ValueToQty = Util.NVC_Decimal(DataTableConverter.GetValue(dgBase.Rows[0].DataItem, "QTY"));

                DataTableConverter.SetValue(dgBase.Rows[5].DataItem, "QTY", ((ValueToQty - 1) / 2));
                DataTableConverter.SetValue(dgBase.Rows[6].DataItem, "QTY", ((ValueToQty + 1) / 2));
                DataTableConverter.SetValue(dgBase.Rows[7].DataItem, "QTY", ((ValueToQty * 3 - 1) / 2));
                DataTableConverter.SetValue(dgBase.Rows[8].DataItem, "QTY", ((ValueToQty * 3 + 1) / 2));

                int rIdx = 0;
                int cIdx = 0;

                C1NumericBox n = sender as C1NumericBox;
                StackPanel panel1 = n.Parent as StackPanel;
                C1.WPF.DataGrid.DataGridCellPresenter p1 = panel1.Parent as C1.WPF.DataGrid.DataGridCellPresenter;
                rIdx = p1.Cell.Row.Index;
                cIdx = p1.Cell.Column.Index;

                if (dgBase.GetRowCount() > ++rIdx)
                {
                    C1.WPF.DataGrid.DataGridCellPresenter p = dgBase.GetCell(rIdx, cIdx).Presenter;
                    StackPanel panel = p.Content as StackPanel;

                    for (int cnt = 0; cnt < panel.Children.Count; cnt++)
                    {
                        if (panel.Children[cnt].Visibility == Visibility.Visible)
                            panel.Children[cnt].Focus();
                    }
                }
            }
        }
        private void txtPKGCELL_GotMouseCapture(object sender, MouseEventArgs e)
        {
            C1.WPF.C1NumericBox nBox = sender as C1.WPF.C1NumericBox;
            nBox.Select(0, nBox.Value.ToString().Length);
        }

        private void txtPKGCELL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                decimal ValueToCELLQty = Util.NVC_Decimal(txtPKGCELL.Value);
                decimal ValueToPKGQty = Util.NVC_Decimal(txtPKGCST.Value);
                decimal ValueToFOLDQty = Util.NVC_Decimal(txtFOLDCELL.Value);
                txtRFOLDCELL.Value = (double)ValueToCELLQty - (double)ValueToPKGQty - (double)ValueToFOLDQty;
            }
        }

        private void txtPKGCST_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                decimal ValueToCELLQty = Util.NVC_Decimal(txtPKGCELL.Value);
                decimal ValueToPKGQty = Util.NVC_Decimal(txtPKGCST.Value);
                decimal ValueToFOLDQty = Util.NVC_Decimal(txtFOLDCELL.Value);
                txtRFOLDCELL.Value = (double)ValueToCELLQty - (double)ValueToPKGQty - (double)ValueToFOLDQty;
            }
        }

        private void txtPKGCST_GotMouseCapture(object sender, MouseEventArgs e)
        {
            C1.WPF.C1NumericBox nBox = sender as C1.WPF.C1NumericBox;
            nBox.Select(0, nBox.Value.ToString().Length);
        }

        private void txtFOLDCELL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                decimal ValueToCELLQty = Util.NVC_Decimal(txtPKGCELL.Value);
                decimal ValueToPKGQty = Util.NVC_Decimal(txtPKGCST.Value);
                decimal ValueToFOLDQty = Util.NVC_Decimal(txtFOLDCELL.Value);
                txtRFOLDCELL.Value = (double)ValueToCELLQty - (double)ValueToPKGQty - (double)ValueToFOLDQty;
            }
        }

        private void txtFOLDCELL_GotMouseCapture(object sender, MouseEventArgs e)
        {
            C1.WPF.C1NumericBox nBox = sender as C1.WPF.C1NumericBox;
            nBox.Select(0, nBox.Value.ToString().Length);
        }

        private void txtQTY_GotMouseCapture(object sender, MouseEventArgs e)
        {
            C1.WPF.C1NumericBox nBox = sender as C1.WPF.C1NumericBox;
            nBox.Select(0, nBox.Value.ToString().Length);
        }

        private void Grid_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                if (Keyboard.FocusedElement is FrameworkElement)
                {
                    e.Handled = true;
                    (Keyboard.FocusedElement as FrameworkElement).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            }
        }
    }
}
