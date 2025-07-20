/*************************************************************************************
 Created Date : 2018.08.10
      Creator : 
   Decription : 노칭대기 전극창고 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2018.08.10  DEVELOPER : Initial Created.

**************************************************************************************/

using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using System.Windows.Media;
using System.Linq;
using C1.WPF.DataGrid;
using C1.WPF;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_247 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();

        DataTable dtiUse = new DataTable();
        DataTable dtType = new DataTable();

        string CSTStatus = string.Empty;

        public COM001_247()
        {
            InitializeComponent();
        }
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Initialize();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            CommonCombo _combo = new CommonCombo();
            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;


            //동           
            C1ComboBox[] cboAreaChild = { cboFloor };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //층
            C1ComboBox[] cboFloorParend = { cboSHOPID, cboArea };
            _combo.SetCombo(cboFloor, CommonCombo.ComboStatus.NONE, cbParent: cboFloorParend);

        }

        private void Init()
        {
            Util.gridClear(dgSearch);
        }
        #endregion

        #region Funct
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

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            getList();
        }
        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowLoadingIndicator();
        }
        private void btnPrj_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_207_PJTLIST popup_PRJ = new COM001_207_PJTLIST();
                popup_PRJ.FrameOperation = this.FrameOperation;

                if (popup_PRJ != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("AREAID", typeof(string));
                    dtData.Columns.Add("WH_ID", typeof(string));

                    DataRow newRow = null;
                    newRow = dtData.NewRow();
                    newRow["AREAID"] = Util.GetCondition(cboArea);
                    newRow["WH_ID"] = Util.GetCondition(cboFloor);
                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup_PRJ, Parameters);
                    //========================================================================

                    popup_PRJ.Closed -= popup_PRJ_Closed;
                    popup_PRJ.Closed += popup_PRJ_Closed;
                    popup_PRJ.ShowModal();
                    popup_PRJ.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void btnLotID_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_207_LOTLIST popup_LOT = new COM001_207_LOTLIST();
                popup_LOT.FrameOperation = this.FrameOperation;

                if (popup_LOT != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("AREAID", typeof(string));
                    dtData.Columns.Add("WH_ID", typeof(string));

                    DataRow newRow = null;
                    newRow = dtData.NewRow();
                    newRow["AREAID"] = Util.GetCondition(cboArea);
                    newRow["WH_ID"] = Util.GetCondition(cboFloor);
                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup_LOT, Parameters);
                    //========================================================================

                    popup_LOT.Closed -= popup_LOT_Closed;
                    popup_LOT.Closed += popup_LOT_Closed;
                    popup_LOT.ShowModal();
                    popup_LOT.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void getList()
        {
            try
            {

                ShowLoadingIndicator();
                
                DataTable inTable = new DataTable();
                inTable.Columns.Add("WH_ID", typeof(string));
                inTable.Columns.Add("PJT", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["WH_ID"] = Util.GetCondition(cboFloor);
                newRow["PJT"] = Util.NVC(txtPrj.Text) == "" ? null : Util.NVC(txtPrj.Text);
                newRow["LOTID"] = Util.NVC(txtLotID.Text) == "" ? null : Util.NVC(txtLotID.Text);
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_NOTCHING_STOCK", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgSearch, dtMain, FrameOperation);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void dgSearch_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "LOTID")
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FIFO_CHECK")).Equals("Y"))
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.DarkSeaGreen);
                        else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "FIFO_CHECK")).Equals("N"))
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "HOT")).Equals("Y"))
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.DarkSeaGreen);
                            else
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                        }
                    }
                }
            }));
        }

        #endregion

        #region POPUP EVENT
        void popup_LOT_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_207_LOTLIST popup = sender as COM001_207_LOTLIST;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    txtLotID.Text = popup.LOTID;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        void popup_PRJ_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_207_PJTLIST popup = sender as COM001_207_PJTLIST;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    txtPrj.Text = popup.PRJ;
                    txtPrj.Tag = popup.PRJ;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion

    }
}

