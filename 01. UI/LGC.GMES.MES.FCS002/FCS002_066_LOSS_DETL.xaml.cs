/*************************************************************************************
 Created Date : 2021.01.14
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.14  DEVELOPER : Initial Created.
  2022.08.31  강동희 : 조회 조건 추가 (동, 공정, Loss Code)




 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_066_LOSS_DETL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string _AREAID = string.Empty;
        string _PROCID = string.Empty;
        string _EQPTID = string.Empty;
        string _LOSSCODE = string.Empty;

        string LossCode = string.Empty;
        string LossDetlCode = string.Empty;

        public FCS002_066_LOSS_DETL()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string _LOSS_CODE
        {
          set { LossCode = value; }
          get { return LossCode; }
        }
        public string _LOSS_DETL_CODE
        {
            set { LossDetlCode = value; }
            get { return LossDetlCode; }
        }
        
        #endregion

        #region Initialize


        #endregion

        #region Event


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _AREAID = Util.NVC(tmps[0]);
                _PROCID = Util.NVC(tmps[1]);
                _EQPTID = Util.NVC(tmps[2]);
                _LOSSCODE = Util.NVC(tmps[3]);
            }

            SetDataGrid();
        }

        private void SetDataGrid()
        {
            try
            {

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("LOSSCODE", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["AREAID"] = _AREAID;
                row["PROCID"] = _PROCID;
                //row["LOSSCODE"] = _LOSSCODE;
                dt.Rows.Add(row);
                
                DataTable tmpResult = new ClientProxy().ExecuteServiceSync("DA_SEL_LOSS_DETL_CODE", "RQST", "RSLT", dt);
                if (tmpResult.Rows.Count == 0) { return; }

                DataTable result = SetDataTable(tmpResult);
                
                dgOne.ItemsSource = result.Select("UPPR_LOSS_CODE = 30000").Length == 0 ? null : DataTableConverter.Convert(result.Select("UPPR_LOSS_CODE = 30000").CopyToDataTable()); //비가동
                dgTwo.ItemsSource = result.Select("UPPR_LOSS_CODE = 20000").Length == 0 ? null : DataTableConverter.Convert(result.Select("UPPR_LOSS_CODE = 20000").CopyToDataTable()); //비부하
                dgThree.ItemsSource = result.Select("UPPR_LOSS_CODE = 10000").Length == 0 ? null : DataTableConverter.Convert(result.Select("UPPR_LOSS_CODE = 10000").CopyToDataTable()); //비조업
                dgFour.ItemsSource = result.Select("UPPR_LOSS_CODE = 40000").Length == 0 ? null : DataTableConverter.Convert(result.Select("UPPR_LOSS_CODE = 40000").CopyToDataTable());//무효가동

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgOne_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            SetRowColor(dataGrid, e);
        }
        private void dgTwo_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            SetRowColor(dataGrid, e);
        }
        private void dgThree_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            SetRowColor(dataGrid, e);
        }
        private void dgFour_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            SetRowColor(dataGrid, e);
        }

        private void SetRowColor(C1.WPF.DataGrid.C1DataGrid dataGrid, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_CODE")).Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_DETL_CODE"))))
                {
                    //색깔바꾸기
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("pink")); ;
                }
            }));
        }

        private DataTable SetDataTable(DataTable tmpResult)
        {
            DataTable result = new DataTable();
            result.Columns.Add("UPPR_LOSS_CODE", typeof(string));
            result.Columns.Add("LOSS_CODE", typeof(string));
            result.Columns.Add("LOSS_DETL_CODE", typeof(string));
            result.Columns.Add("LOSS_DETL_NAME", typeof(string));


            int diffIndex = 0;
            DataRow row = null;
            string pre_loss_code = "";

            while (true)
            {
                row = result.NewRow();

                row["UPPR_LOSS_CODE"] = Convert.ToString(tmpResult.Rows[diffIndex]["UPPR_LOSS_CODE"]);
                row["LOSS_CODE"] = Convert.ToString(tmpResult.Rows[diffIndex]["LOSS_CODE"]);
                row["LOSS_DETL_CODE"] = Convert.ToString(tmpResult.Rows[diffIndex]["LOSS_CODE"]);
                row["LOSS_DETL_NAME"] = Convert.ToString(tmpResult.Rows[diffIndex]["LOSS_NAME"]);
                result.Rows.Add(row);

                pre_loss_code = Convert.ToString(tmpResult.Rows[diffIndex]["LOSS_CODE"]);


                for (int i = diffIndex; i < tmpResult.Rows.Count; i++)
                {

                    if (!Convert.ToString(tmpResult.Rows[i]["LOSS_CODE"]).Equals(pre_loss_code))
                    {
                        diffIndex = i;
                        break;
                    }
                    row = result.NewRow();

                    row["UPPR_LOSS_CODE"] = Convert.ToString(tmpResult.Rows[i]["UPPR_LOSS_CODE"]);
                    row["LOSS_CODE"] = Convert.ToString(tmpResult.Rows[i]["LOSS_CODE"]);
                    row["LOSS_DETL_CODE"] = Convert.ToString(tmpResult.Rows[i]["LOSS_DETL_CODE"]);
                    row["LOSS_DETL_NAME"] = Convert.ToString(tmpResult.Rows[i]["LOSS_DETL_NAME"]);

                    result.Rows.Add(row);
                    pre_loss_code = Convert.ToString(tmpResult.Rows[i]["LOSS_CODE"]);

                    if (i == tmpResult.Rows.Count - 1)
                    {
                        return result;
                    }

                }

            }
        }

        private void dgOne_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;


            int index = (sender as C1.WPF.DataGrid.C1DataGrid).CurrentCell.Row.Index;
            C1.WPF.DataGrid.C1DataGrid datagrid = sender as C1.WPF.DataGrid.C1DataGrid;
            if (Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "LOSS_CODE")).Equals(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "LOSS_DETL_CODE"))))
            {
                return;
            }

            _LOSS_CODE = Util.NVC(DataTableConverter.GetValue((sender as C1.WPF.DataGrid.C1DataGrid).CurrentCell.Row.DataItem, "LOSS_CODE"));
            _LOSS_DETL_CODE = Util.NVC(DataTableConverter.GetValue((sender as C1.WPF.DataGrid.C1DataGrid).CurrentCell.Row.DataItem, "LOSS_DETL_CODE"));

            this.DialogResult = MessageBoxResult.OK;

        }
       

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) { return; }
            this.DialogResult = MessageBoxResult.OK;
        }



        #endregion
     
       
    }
}
