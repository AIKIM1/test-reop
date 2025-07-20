/*************************************************************************************
 Created Date : 2020.11.04
      Creator : 주건태
   Decription : 패키징(파우치 2D) LOT추적
--------------------------------------------------------------------------------------
 [Change History]
  
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_339 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public COM001_339()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (ValidationLotTraceInput())
            {
                GetLotTraceList();
            }
        }

        private void txtLotId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (ValidationLotTraceInput())
                    {
                        GetLotTraceList();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoProdLot_Click(object sender, RoutedEventArgs e)
        {
            SetIsEnabledDatePicker();
        }

        private void rdoOutLotId_Click(object sender, RoutedEventArgs e)
        {
            SetIsEnabledDatePicker();
        }

        private void rdoCstId_Click(object sender, RoutedEventArgs e)
        {
            SetIsEnabledDatePicker();
        }

        private void rdoCELLId_Click(object sender, RoutedEventArgs e)
        {
            SetIsEnabledDatePicker();
        }

        #endregion

        private void SetIsEnabledDatePicker()
        {
            if (rdoCstId.IsChecked == true)
            {
                spDate.IsEnabled = true;
            }
            else
            {
                spDate.IsEnabled = false;
            }
        }

        private bool ValidationLotTraceInput()
        {
            if (String.IsNullOrEmpty(txtLotId.Text))
            {
                Util.MessageValidation("SFU2052");  //입력된 항목이 없습니다.
                txtLotId.Focus();
                return false;
            }
            return true;
        }

        private void GetLotTraceList()
        {
            try
            {
                Util.gridClear(dgLotTrace);

                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("OUT_LOTID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                if (rdoProdLot.IsChecked == true)
                {
                    dr["PROD_LOTID"] = Util.NVC(txtLotId.Text);
                }
                else if (rdoOutLotId.IsChecked == true)
                {
                    dr["OUT_LOTID"] = Util.NVC(txtLotId.Text);
                }
                else if (rdoCstId.IsChecked == true)
                {
                    dr["CSTID"] = Util.NVC(txtLotId.Text);
                    dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                    dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                }
                else if (rdoCELLId.IsChecked == true)
                {
                    dr["SUBLOTID"] = Util.NVC(txtLotId.Text);
                }

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOT_TRACE_CL_2D", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt != null && dtRslt.Rows.Count >= 0)
                {
                    Util.GridSetData(dgLotTrace, dtRslt, FrameOperation, true);
                }

                /*
                new ClientProxy().ExecuteService("DA_BAS_SEL_LOT_TRACE_CL_2D", "INDATA", "OUTDATA", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgLotTrace, bizResult, FrameOperation, true);
                });
                */

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

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

        private void rdoSTPProdLot_Click(object sender, RoutedEventArgs e)
        {
            SetIsEnabledSTPDatePicker();
        }

        private void rdoSTPOutLotId_Click(object sender, RoutedEventArgs e)
        {
            SetIsEnabledSTPDatePicker();
        }

        private void rdoSTPCstId_Click(object sender, RoutedEventArgs e)
        {
            SetIsEnabledSTPDatePicker();
        }

        private void rdoSTPCELLId_Click(object sender, RoutedEventArgs e)
        {
            SetIsEnabledSTPDatePicker();
        }

        private void SetIsEnabledSTPDatePicker()
        {
            if (rdoSTPCstId.IsChecked == true)
            {
                spSTPDate.IsEnabled = true;
            }
            else
            {
                spSTPDate.IsEnabled = false;
            }
        }

        private void txtSTPLotId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (ValidationSTPLotTraceInput())
                    {
                        GetSTPLotTraceList();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidationSTPLotTraceInput()
        {
            if (String.IsNullOrEmpty(txtSTPLotId.Text))
            {
                Util.MessageValidation("SFU2052");  //입력된 항목이 없습니다.
                txtSTPLotId.Focus();
                return false;
            }
            return true;
        }

        private void btnSTPSearch_Click(object sender, RoutedEventArgs e)
        {
            if (ValidationSTPLotTraceInput())
            {
                GetSTPLotTraceList();
            }
        }

        private void GetSTPLotTraceList()
        {
            try
            {
                Util.gridClear(dgSTPLotTrace);

                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("OUT_LOTID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                if (rdoSTPProdLot.IsChecked == true)
                {
                    dr["PROD_LOTID"] = Util.NVC(txtSTPLotId.Text);
                }
                else if (rdoSTPOutLotId.IsChecked == true)
                {
                    dr["OUT_LOTID"] = Util.NVC(txtSTPLotId.Text);
                }
                else if (rdoSTPCstId.IsChecked == true)
                {
                    dr["CSTID"] = Util.NVC(txtSTPLotId.Text);
                    dr["FROM_DATE"] = Util.GetCondition(dtpSTPDateFrom);
                    dr["TO_DATE"] = Util.GetCondition(dtpSTPDateTo);
                }
                else if (rdoSTPCELLId.IsChecked == true)
                {
                    dr["SUBLOTID"] = Util.NVC(txtSTPLotId.Text);
                }

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOT_TRACE_STP_2D", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt != null && dtRslt.Rows.Count >= 0)
                {
                    Util.GridSetData(dgSTPLotTrace, dtRslt, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

    }
}