/*************************************************************************************
 Created Date : 2016.08.18
      Creator :
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.18  
  2017.01.23  
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_SHIFT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_COST_CNTR : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _Shop = string.Empty;
        private string _sCenterNameLike = string.Empty;

        private string _COST_CNTR_ID = string.Empty;
        private string _COST_CNTR_NAME = string.Empty;

        Util _Util = new Util();

        #endregion
        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public CMM_COST_CNTR()
        {
            InitializeComponent();
            _COST_CNTR_ID = string.Empty;
            _COST_CNTR_NAME = string.Empty;
            _Shop = string.Empty;
            _sCenterNameLike = string.Empty;
        }

        private void InitializeControls()
        {
            
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
          
            if (tmps != null && tmps.Length >= 1)
            {
                _Shop = Util.NVC(tmps[0]);
            }
        }
        public string COST_CNTR_ID
        {
            get { return _COST_CNTR_ID; }
        }
        public string COST_CNTR_NAME
        {
            get { return _COST_CNTR_NAME; }
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            CNTR_Search();
            
        }

        private void dgCostCNTRChoice_Checked(object sender, RoutedEventArgs e)
        {

            RadioButton rb = sender as RadioButton;
            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                //DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                dgCostCNTR.SelectedIndex = idx;
                _COST_CNTR_ID = DataTableConverter.GetValue(dgCostCNTR.Rows[idx].DataItem, "COST_CNTR_ID").ToString();
                _COST_CNTR_NAME = DataTableConverter.GetValue(dgCostCNTR.Rows[idx].DataItem, "COST_CNTR_NAME").ToString();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            if (dgCostCNTR.SelectedIndex < 0)
            {
                Util.MessageInfo("SFU1275");//선택된 항목이 없습니다.
                return;
            }
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        
        private void txtCenterName_KeyUp(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{
            //    _sCenterNameLike = txtCenterName.Text.Trim();
            //}
        }

        #endregion

        private void txtCenterName_KeyDown(object sender, KeyEventArgs e)
        {     
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(txtCenterName.Text))
                {                   
                    txtCenterName.Focus();
                    return;
                }
                CNTR_Search();
            }

        }

        private void CNTR_Search()
        {
            try
            {
                if (txtCenterName.Text.Trim().Equals(""))
                {
                    _sCenterNameLike = null;
                }
                else
                {
                    _sCenterNameLike = txtCenterName.Text.Trim();
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("COST_CNTR_LIKE_NAME", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["COST_CNTR_LIKE_NAME"] = _sCenterNameLike;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST_COST_CENTER", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgCostCNTR, dtResult, FrameOperation);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}