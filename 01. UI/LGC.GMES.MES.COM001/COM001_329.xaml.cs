/*************************************************************************************
 Created Date : 2020.07.30
      Creator : 김대근(CNS)
   Decription : 투입LOT 종료취소 이력
--------------------------------------------------------------------------------------
 [Change History]
  2020.07.30 CMI 요청 : 투입LOT 종료취소 이력
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_329 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        public COM001_329()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Now.AddMonths(-1);
            dtpDateTo.SelectedDateTime = DateTime.Now;

            tbArea.Text = LoginInfo.CFG_AREA_ID + " : " + LoginInfo.CFG_AREA_NAME;

            CommonCombo combo = new CommonCombo();
            string[] filter = { LoginInfo.CFG_AREA_ID, "P", "A,E" };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, sFilter: filter, sCase: "PROCESS_BY_AREAID_PCSG_ETC");
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (loadingIndicator.Visibility != Visibility.Visible)
                loadingIndicator.Visibility = Visibility.Visible;
            GetInputLotCancelTermList();
        }

        private void GetInputLotCancelTermList()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("STRTDTTM", typeof(DateTime));
                inTable.Columns.Add("ENDDTTM", typeof(DateTime));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Util.NVC(cboProcess.SelectedValue).Equals("") ? null : Util.NVC(cboProcess.SelectedValue);
                newRow["STRTDTTM"] = dtpDateFrom.SelectedDateTime.Date;
                newRow["ENDDTTM"] = dtpDateTo.SelectedDateTime.Date.AddDays(1);
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_INPUT_LOT_CANCEL_TERM_LIST", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try { Util.GridSetData(dgList, bizResult, FrameOperation); }
                    catch (Exception ex) { throw ex; }
                    finally
                    {
                        if (loadingIndicator.Visibility == Visibility.Visible)
                            loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                if (loadingIndicator.Visibility == Visibility.Visible)
                    loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
    }
}