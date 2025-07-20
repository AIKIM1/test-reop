/*************************************************************************************
 Created Date : 2017.09.28
      Creator : 
   Decription : 출하처선택
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using Application = System.Windows.Application;
using DataGridLength = C1.WPF.DataGrid.DataGridLength;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Linq;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_SHIPTO : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        string _shipTo;

        public string ShipTo_ID { get; set; }
        public string ShipTo_Name { get; set; }

        private bool _load = true;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public FCS002_SHIPTO()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();
                SetControlVisibility();
                _load = false;
            }

        }
        private void InitializeUserControls()
        {
        }

        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _shipTo = tmps[0] as string;

            //  출하처 조회
            SetShipTo();

        }

        private void SetControlVisibility()
        {
        }

        #endregion

        #region [출하처 그리드에서 선택]
        private void dgShipToChoice1_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK1"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK1"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK1", idx == i);
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK2", 0);
                        }

                        dgShipTo.Selection.Clear();

                        ////row 색 바꾸기
                        //dgShipTo.SelectedIndex = idx;
                    }
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

        private void dgShipToChoice2_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;
                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK2"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK2"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK1", 0);
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK2", idx == i);
                        }

                        dgShipTo.Selection.Clear();

                        ////row 색 바꾸기
                        //dgShipTo.SelectedIndex = idx;
                    }
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

        private void dgShipTo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.DataItem == null)
                    return;

                if (e.Cell.Column.Name.Equals("CHK2"))
                {
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "SHIPTO_ID2") == null)
                    {
                        //e.Cell.Presenter.Visibility = Visibility.Collapsed;
                        e.Cell.Presenter.IsEnabled = false;
                    }
                }

            }));

        }

        #endregion

        #region [선택]
        /// <summary>
        /// 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateSelect())
                return;

            this.DialogResult = MessageBoxResult.OK;

        }
        #endregion

        #region [닫기]
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region User Method

        #region [BizCall]
        /// <summary>
        /// 출하처 조회
        /// </summary>
        private void SetShipTo()
        {
            try
            {
                ShowLoadingIndicator();

                // DATA Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SHIPTO_FO", "RQSTDT", "RSLTDT", inTable);

                if (!string.IsNullOrWhiteSpace(_shipTo))
                {
                    dtResult.Select("SHIPTO_ID1 = '" + _shipTo + "'").ToList<DataRow>().ForEach(r => r["CHK1"] = true);
                    dtResult.Select("SHIPTO_ID2 = '" + _shipTo + "'").ToList<DataRow>().ForEach(r => r["CHK2"] = true);
                    dtResult.AcceptChanges();
                }

                Util.GridSetData(dgShipTo, dtResult, null, true);

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region[[Validation]
        private bool ValidateSelect()
        {
            DataTable dt = DataTableConverter.Convert(dgShipTo.ItemsSource);

            DataRow[] dr = dt.Select("CHK1 = 1 OR CHK2 = 1");

            if (dr.Length == 0)
            {
                // "선택된 항목이 없습니다."
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (dr[0]["CHK1"].ToString() == "1")
            {
                ShipTo_ID = dr[0]["SHIPTO_ID1"].ToString();
                ShipTo_Name = dr[0]["SHIPTO_NAME1"].ToString();
            }
            else
            {
                ShipTo_ID = dr[0]["SHIPTO_ID2"].ToString();
                ShipTo_Name = dr[0]["SHIPTO_NAME2"].ToString();
            }

            return true;
        }
        #endregion

        #region [Func]
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

        #endregion

    }
}
