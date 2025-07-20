/*************************************************************************************
 Created Date : 2017.10.09
      Creator : 신광희C
   Decription : 전지 5MEGA-GMES 구축 - Washing 공정진척 화면 - BOX 발행 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.10.09  신광희C : Initial Created.
  
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using System.Data;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_BOX_HISTORYCARD.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_BOX_HISTORYCARD : C1Window, IWorkArea
    {
        #region Declaration & Constructor         

        private BizDataSet _bizDataSet = new BizDataSet();
        private Util _util = new Util();
        private string _processCode = string.Empty;

        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public CMM_ASSY_BOX_HISTORYCARD()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
                //InitializeControls();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                _processCode = Util.NVC(tmps[0]);

                ApplyPermissions();
                InitializeControls();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearchBoxPrint())
                return;
            SearchBoxPrint();
            
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn != null)
                {
                    string lotId = Util.NVC(DataTableConverter.GetValue(btn.DataContext, "LOTID"));

                    if (!string.IsNullOrEmpty(lotId))
                    {
                        PrintBox(lotId);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        private bool ValidationSearchBoxPrint()
        {
            DateTime dtEndTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, dtpDateTo.SelectedDateTime.Day);
            DateTime dtStartTime = new DateTime(dtpDateFrom.SelectedDateTime.Year, dtpDateFrom.SelectedDateTime.Month, dtpDateFrom.SelectedDateTime.Day);

            if (!(Math.Truncate(dtEndTime.Subtract(dtStartTime).TotalSeconds) >= 0))
            {
                //종료일자가 시작일자보다 빠릅니다.
                Util.MessageValidation("SFU1913");
                return false;
            }
            return true;
        }

        private void SearchBoxPrint()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "DA_PRD_SEL_BOX_LIST_WS";
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));
                inDataTable.Columns.Add("LOTID_AS", typeof(string));
                inDataTable.Columns.Add("LOTID_BX", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToShortDateString();
                dr["LOTID_AS"] = txtAssemblyLotID.Text;
                dr["LOTID_BX"] = txtBoxID.Text;
                dr["EQSGID"] = cboEquipmentSegmentAssy.SelectedValue;
                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (searchResult, searchException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        //dgBox.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgBox, searchResult, FrameOperation, true);
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
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSave);
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();
            const string bizRuleName = "BR_CUS_GET_SYSTIME";

            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }

        private void InitializeControls()
        {
            SetEquipmentSegmentCombo(cboEquipmentSegmentAssy);

            if (dtpDateFrom != null)
            {
                dtpDateFrom.SelectedDateTime = GetSystemTime().AddDays(-7);
                dtpDateFrom.Text = GetSystemTime().AddDays(-7).ToShortDateString();
            }


            if (dtpDateTo != null)
            {
                dtpDateTo.SelectedDateTime = GetSystemTime();
                dtpDateTo.Text = GetSystemTime().ToShortDateString();
            }
        }

        private void PrintBox(string lotId)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                const string bizRuleName = "DA_PRD_SEL_BOX_RUNCARD_DATA_WS";
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                DataRow indata = inDataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["LOTID"] = lotId;
                inDataTable.Rows.Add(indata);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (CommonVerify.HasTableRow(result))
                    {
                        CMM_ASSY_BOXCARD_PRINT poopupHistoryCard = new CMM_ASSY_BOXCARD_PRINT { FrameOperation = this.FrameOperation };
                        object[] parameters = new object[1];
                        parameters[0] = result;
                        C1WindowExtension.SetParameters(poopupHistoryCard, parameters);
                        poopupHistoryCard.Closed += new EventHandler(poopupHistoryCard_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => poopupHistoryCard.ShowModal()));
                    }
                    else
                    {
                        //데이터가 없습니다.
                        Util.MessageValidation("SFU1498");
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void poopupHistoryCard_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_BOXCARD_PRINT popup = sender as CMM_ASSY_BOXCARD_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            ////원각 : CR , 초소형 : CS
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_CR";
            string[] arrColumn = { "LANGID", "AREAID", "PROD_GROUP", "PROCID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "CR", _processCode };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        #endregion


    }
}
