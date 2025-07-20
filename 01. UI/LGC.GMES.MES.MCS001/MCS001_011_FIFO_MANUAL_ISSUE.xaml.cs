/*************************************************************************************
 Created Date : 2019.03.26
      Creator : 신광희 차장
   Decription : CWA물류-전극 FIFO 수동 출고 예약 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2019.03.26  신광희 차장 : Initial Created.      
**************************************************************************************/

using System;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_011_FIFO_MANUAL_ISSUE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_011_FIFO_MANUAL_ISSUE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        public IFrameOperation FrameOperation { get; set; }
        public bool IsUpdated;
        private readonly Util _util = new Util();
        
        #endregion

        public MCS001_011_FIFO_MANUAL_ISSUE()
        {
            InitializeComponent();
            InitializeControls();
        }

        #region Initialize 
        private void InitializeControls()
        {
            InitializeCombo();
        }

        private void InitializeCombo()
        {
            //공정 콤보박스
            SetProcessCombo(cboProcess);

            //무지부 콤보박스
            SetWorkHalfSlittingSideCombo(cboWorkHalfSlittingSide);
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            //cboProcess.SelectedValue = parameters[0];
            if (!string.IsNullOrEmpty(parameters?[1].GetString()))
            {
                if (parameters[1].GetString() == "C")
                    rdoAnode.IsChecked = true;
                else
                    rdoCathode.IsChecked = true;

                SetDestinationCombo(cboDestination);
            }
            btnSearch_Click(btnSearch, null);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_MCS_SEL_STK_JUMBOROLL_OUT_LIST";
                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    var dtBinding = bizResult.Copy();
                    dtBinding.Columns.Add(new DataColumn() { ColumnName = "SEQ", DataType = typeof(int) });
                    int rowIndex = 1;

                    foreach (DataRow row in dtBinding.Rows)
                    {
                        row["SEQ"] = rowIndex;
                        rowIndex++;
                    }
                    dtBinding.AcceptChanges();
                    Util.GridSetData(dgList, dtBinding, null, true);
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void btnFiFoIssue_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationFiFoIssue()) return;
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_MCS_REG_LOGIS_CMD_FIFO";

                DataTable dt = DataTableConverter.Convert(cboDestination.ItemsSource);
                DataRow dr = dt.Rows[cboDestination.SelectedIndex];

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                inTable.Columns.Add("HALF_SLIT_SIDE", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("TO_PORT_ID", typeof(string));
                inTable.Columns.Add("PORT_WRK_MODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));


                string halfSlitSide = null;

                if(rdoAnode.IsChecked != null && (rdoAnode != null && (bool) rdoAnode.IsChecked))
                {
                    halfSlitSide = cboWorkHalfSlittingSide.SelectedValue.GetString();
                }


                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["PRJT_NAME"] = txtProjectName.Text;
                newRow["PROD_VER_CODE"] = txtProductVersion.Text;
                newRow["HALF_SLIT_SIDE"] = halfSlitSide;
                newRow["PROCID"] = cboProcess.SelectedValue;
                newRow["TO_PORT_ID"] = cboDestination.SelectedValue;
                newRow["PORT_WRK_MODE"] = dr["PORT_WRK_MODE"].GetString();
                newRow["USERID"] = LoginInfo.USERID;
                
                inTable.Rows.Add(newRow);

                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", "OUTDATA,OUTMSG", (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        IsUpdated = true;
                        btnSearch_Click(btnSearch, null);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, ds);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void rdoAnode_Checked(object sender, RoutedEventArgs e)
        {
            cboWorkHalfSlittingSide.IsEnabled = true;
            SetDestinationCombo(cboDestination);
        }

        private void rdoCathode_Checked(object sender, RoutedEventArgs e)
        {
            cboWorkHalfSlittingSide.IsEnabled = false;
            SetDestinationCombo(cboDestination);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #region Mehod


        private bool ValidationFiFoIssue()
        {

            if (string.IsNullOrEmpty(txtProjectName.Text.Trim()))
            {
                Util.MessageValidation("SFU3478");  //PJT을 선택하세요.
                return false;
            }

            if (string.IsNullOrEmpty(txtProductVersion.Text.Trim()))
            {
                Util.MessageValidation("SFU1563");  //버전 정보를 지정하세요.
                return false;
            }
            

            if (string.IsNullOrEmpty(cboProcess.SelectedValue.GetString()))
            {
                Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                return false;
            }

            if (string.IsNullOrEmpty(cboDestination?.SelectedValue?.GetString()))
            {   
                Util.MessageValidation("SFU5061");  //PORT가 설정되지 않았습니다.
                return false;
            }

            if (rdoAnode.IsChecked != null && (rdoAnode.IsChecked == false && rdoCathode.IsChecked == false))
            {
                Util.MessageValidation("SFU1467");  //극성을 선택 하세요.
                return false;
                
            }

            if (rdoAnode.IsChecked != null && (bool) rdoAnode.IsChecked)
            {
                if (cboWorkHalfSlittingSide?.SelectedValue == null || cboWorkHalfSlittingSide.SelectedValue.GetString() == "SELECT")
                {
                    Util.MessageValidation("SFU6023");  //무지부를 선택 하세요.
                    return false;
                }
            }

            return true;
        }

        private static void SetProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_PROCESS_CBO";
            string[] arrColumn = { "LANGID", "EQSGID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_EQSG_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetDestinationCombo(C1ComboBox cbo)
        {

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["ELTR_TYPE_CODE"] = rdoAnode.IsChecked != null && (bool) rdoAnode.IsChecked ? "C" : "A";
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_PORT_RELATION_FIFO_CBO", "RQSTDT", "RSLTDT", inTable);

            cbo.ItemsSource = dtResult.Copy().AsDataView();
            cbo.SelectedValue = LoginInfo.CFG_EQPT_ID;
            if (cbo.SelectedIndex < 0) cbo.SelectedIndex = 0;
        }

        private static void SetWorkHalfSlittingSideCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "WRK_HALF_SLIT_SIDE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null && (loadingIndicator != null || loadingIndicator.Visibility != Visibility.Visible))
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null && loadingIndicator.Visibility == Visibility.Visible)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }




        #endregion


    }
}
