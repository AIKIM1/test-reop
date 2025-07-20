/*************************************************************************************
 Created Date : 2021.04.28
      Creator : 김태균
   Decription : 외부 창고 현황
--------------------------------------------------------------------------------------
 [Change History]
  2021.04.28  NAME : Initial Created

**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Popup;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;


namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_112 : UserControl,IWorkArea
    {
        #region [Declaration & Constructor]
        Util _Util = new Util();
       
        #endregion

        #region [Initialize]
        public FCS001_112()
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
            InitCombo();
            
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            // 동
            ComCombo.SetCombo(cboArea, CommonCombo_Form.ComboStatus.NONE, sCase: "AREA");

            // 라인
            SetEquipmentSegmentCombo(cboLine);
            
            // Lot 유형
            string[] sFilter1 = { "LOT_DETL_TYPE_CODE" };
            ComCombo.SetCombo(cboLotType, CommonCombo_Form.ComboStatus.ALL, sCase: "FORM_CMN", sFilter: sFilter1);
        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                Util.gridClear(dgPalletList);
                Util.gridClear(dgCellList);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = Util.NVC(cboLine.SelectedValue).Equals(string.Empty) ? null : Util.GetCondition(cboLine);
                newRow["LOT_DETL_TYPE_CODE"] = Util.NVC(cboLotType.SelectedValue).Equals(string.Empty) ? null : Util.GetCondition(cboLotType);

                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_WH_INFO", "RQSTDT", "RSLTDT", inTable);

                Util.GridSetData(dgPalletList, dtRslt, FrameOperation, true);

                string[] sColumnName = new string[] { "OUTER_WH_PLLT_ID" };
                _Util.SetDataGridMergeExtensionCol(dgPalletList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetDetailInfo(string sPlltID)
        {
            try
            {
                Util.gridClear(dgCellList);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("OUTER_WH_PLLT_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["OUTER_WH_PLLT_ID"] = Util.NVC(sPlltID);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_PLLT_CELL_INFO", "RQSTDT", "RSLTDT", inTable);

                Util.GridSetData(dgCellList, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMBO_FORM_LINE";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo_Form.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo_Form.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }
        
        #endregion

        #region [Event]
        
        private void btnSearch_Click(object sender, EventArgs e)
        {
            GetList();
        }

        private void dgPalletList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgPalletList == null || dgPalletList.CurrentRow == null || !dgPalletList.Columns.Contains("CSTID") )
                    return;
                if (dgPalletList.CurrentColumn.Name.Equals("CSTID"))
                {
                    FCS001_021 wndTRAY = new FCS001_021();
                    wndTRAY.FrameOperation = FrameOperation;

                    object[] Parameters = new object[10];
                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgPalletList.CurrentRow.DataItem, "CSTID")).ToString(); // TRAY ID
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgPalletList.CurrentRow.DataItem, "LOTID")).ToString(); // LOTID

                    this.FrameOperation.OpenMenu("SFU010710010", true, Parameters);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgPalletList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;

                    if (e.Cell.Column.Name.Equals("CSTID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    
                }));

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        
        private void dgPalletList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgPalletList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string sPlltID = Util.NVC(DataTableConverter.GetValue(dgPalletList.Rows[cell.Row.Index].DataItem, "OUTER_WH_PLLT_ID"));

                    GetDetailInfo(sPlltID);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region Method
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

        
    }
}
