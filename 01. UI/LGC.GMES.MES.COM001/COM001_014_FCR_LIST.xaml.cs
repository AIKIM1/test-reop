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
using System.Windows.Input;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_014_FCR_LIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string area = string.Empty;
        string process = string.Empty;
        string eqsgid = string.Empty;

        string fcr_gr_code = string.Empty;
        string f_code = string.Empty;
        string c_code = string.Empty;
        string r_code = string.Empty;




        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();

        public COM001_014_FCR_LIST()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public string FCR_GR_CODE
        {
            get { return fcr_gr_code; }
            set { fcr_gr_code = value; }
        }
        public string F_CODE
        {
            get { return f_code; }
            set { f_code = value; }
        }
        public string C_CODE
        {
            get { return c_code; }
            set { c_code = value; }
        }
        public string R_CODE
        {
            get { return r_code; }
            set { r_code = value; }
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
                area = Util.NVC(tmps[0]);
                process = Util.NVC(tmps[1]);
                eqsgid = Util.NVC(tmps[2]);

            }


            try
            {

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["AREAID"] = area;
                row["PROCID"] = process;

                dt.Rows.Add(row);


                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREA_EQPT_LOSS", "RQST", "RSLT", dt);
                if (result.Rows.Count == 0) { return; }

                txtArea.Text = Convert.ToString(result.Rows[0]["AREANAME"]);
                txtProcess.Text = Convert.ToString(result.Rows[0]["PROCNAME"]);

                getFCRList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            

        
        }

        private void dgFCRChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && ((rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0")|| (rb.DataContext as DataRowView).Row["CHK"].Equals(false)))
            {
                int index = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (index == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                
                FCR_GR_CODE = Util.NVC(DataTableConverter.GetValue(dgFCR.Rows[index].DataItem, "FCR_GR_CODE"));
                F_CODE = Util.NVC(DataTableConverter.GetValue(dgFCR.Rows[index].DataItem, "F_CODE"));
                C_CODE = Util.NVC(DataTableConverter.GetValue(dgFCR.Rows[index].DataItem, "C_CODE"));
                R_CODE = Util.NVC(DataTableConverter.GetValue(dgFCR.Rows[index].DataItem, "R_CODE"));

                txtFailure.Text = Util.NVC(DataTableConverter.GetValue(dgFCR.Rows[index].DataItem, "FCR_CODE_NAME_F"));
                txtCause.Text = Util.NVC(DataTableConverter.GetValue(dgFCR.Rows[index].DataItem, "FCR_CODE_NAME_C"));
                txtResolution.Text = Util.NVC(DataTableConverter.GetValue(dgFCR.Rows[index].DataItem, "FCR_CODE_NAME_R"));
            }
        }
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) { return; }

            if (_Util.GetDataGridCheckFirstRowIndex(dgFCR, "CHK") == -1)
            {
                Util.MessageValidation("SFU4129");//FCR그룹코드를 선택해주세요
                return;
            }

            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgFCR, "CHK") == -1)
            {
                Util.MessageValidation("SFU4129");//FCR그룹코드를 선택해주세요
                return;
            }

            //FCR그룹코드를 삭제하시겠습니까?
            Util.MessageConfirm("SFU4130", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        if (loadingIndicator != null)
                            loadingIndicator.Visibility = Visibility.Visible;

                        DataSet ds = new DataSet();
                        DataTable dt = ds.Tables.Add("INDATA");
                        dt.Columns.Add("AREAID", typeof(string));
                        dt.Columns.Add("PROCID", typeof(string));
                        dt.Columns.Add("FCR_GR_CODE", typeof(string));
                        dt.Columns.Add("USERID", typeof(string));

                        DataRow row = dt.NewRow();
                        row["AREAID"] = area;
                        row["PROCID"] = process;
                        row["FCR_GR_CODE"] = Util.NVC(DataTableConverter.GetValue(dgFCR.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFCR, "CHK")].DataItem, "FCR_GR_CODE"));//Util.NVC(DataTableConverter.GetValue(dgFCR.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFCR, "CHK")].DataItem, "FCR_GR_CODE"));
                        row["USERID"] = LoginInfo.USERID;
                        dt.Rows.Add(row);

                        new ClientProxy().ExecuteService_Multi("BR_EQPT_EQPTLOSS_FCRGR_DELETE", "INDATA", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }


                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                getFCRList();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                if (loadingIndicator != null)
                                    loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, ds);

                    }
                });
         
        }

        #endregion

        #region Mehod


        private void getFCRList()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["AREAID"] = area;
                row["PROCID"] = process;
                dt.Rows.Add(row);

                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("DA_EQP_SEL_EQPTLOSS_FCR_GR", "INDATA", "RSLT", dt, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgFCR, bizResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        if (loadingIndicator != null)
                            loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });


            }
            catch (Exception ex)
            {

            }
        }














        #endregion

    }
}
