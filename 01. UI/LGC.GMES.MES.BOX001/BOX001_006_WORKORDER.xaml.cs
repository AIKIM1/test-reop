/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_006_WORKORDER : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        DataTable dtWO = new DataTable();
        private string sAREAID = string.Empty;
        private string sEQSGID = string.Empty;
        private string sPROCID = string.Empty;
        private string sMDLLOT_ID = string.Empty;

        // 팝업 호출한 폼으로 리턴함.
        private DataTable _RetDT = null;
        public DataTable retDT
        {
            get { return _RetDT; }
        }

        public BOX001_006_WORKORDER()
        {
            InitializeComponent();
            Loaded += BOX001_006_WORKORDER_Loaded;
        }


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

        private void BOX001_006_WORKORDER_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            sAREAID = tmps[0] as string;
            sEQSGID = tmps[1] as string;
            sPROCID = tmps[2] as string;
            sMDLLOT_ID = tmps[3] as string;

            dtpDateFrom.Text = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            dtpDateTo.Text = DateTime.Now.ToString("yyyy-MM-dd");

        }

        #endregion

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Get_WOList();
        }

        private void dgWOChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {

                //selWOData = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.DataItem;
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                //DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);

                //if (dt != null)
                //{
                //    for (int i = 0; i < dt.Rows.Count; i++)
                //    {
                //        DataRow row = dt.Rows[i];

                //        if (idx == i)
                //            dt.Rows[i]["CHK"] = true;
                //        else
                //            dt.Rows[i]["CHK"] = false;
                //    }
                //    dgWO.BeginEdit();
                //    dgWO.ItemsSource = DataTableConverter.Convert(dt);
                //    dgWO.EndEdit();
                //}

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            int iRow = -1;

            DataTable DT = DataTableConverter.Convert(dgWO.ItemsSource);
            for (int i = 0; i < DT.Rows.Count; i++)
            {
                if (Util.NVC(DT.Rows[i]["CHK"]) == "1")
                {
                    iRow = i;
                    break;
                }
            }

            if (iRow == -1)
            {
                Util.MessageValidation("SFU1629"); //"선택 후 작업하세요"
                return;
            }
            else
            {

                DataTable dt = dtWO.Clone();
                DataRow dr = dtWO.Rows[iRow];
                dt.ImportRow(dr);

                _RetDT = dt;

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        #endregion


        #region Mehod

        private void Get_WOList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = sAREAID;
                //dr["EQSGID"] = sEQSGID;
                //dr["PROCID"] = sPROCID;
                dr["MDLLOT_ID"] = sMDLLOT_ID;
                RQSTDT.Rows.Add(dr);

                dtWO = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WO_CP", "RQSTDT", "RSLTDT", RQSTDT);
                ////dgWO.ItemsSource = DataTableConverter.Convert(dtWO);
                Util.GridSetData(dgWO, dtWO, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }







        #endregion

       
    }
}
