/*************************************************************************************
 Created Date : 2021.03.24
      Creator : 신광희
   Decription : 자동차 전극 Roll Map - Roll Map 데이터 이력 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2021.03.24  신광희 : Initial Created.
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_354_ROLLMAP_CLCT_INFO_HIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _equipmentCode;
        private string _equipmentMeasurementPositionCode;
        private string _lotId;
        private string _wipSeq;
        private string _collectSeq;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_354_ROLLMAP_CLCT_INFO_HIST()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {

            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null)
            {
                _equipmentCode = Util.NVC(parameters[0]);
                _equipmentMeasurementPositionCode = Util.NVC(parameters[1]);
                _lotId = Util.NVC(parameters[2]);
                _wipSeq = Util.NVC(parameters[3]);
                _collectSeq = Util.NVC(parameters[4]);

                GetRollMapCollectInfoHistoryData();
            }

        }
        #endregion

        #region Event
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod
        private void GetRollMapCollectInfoHistoryData()
        {
            const string bizRuleName = "DA_PRD_SEL_ROLLMAP_COLLECT_INFO_HIST";

            loadingIndicator.Visibility = Visibility.Visible;

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(string));
            inTable.Columns.Add("CLCT_SEQNO", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["EQPTID"] = _equipmentCode;
            dr["EQPT_MEASR_PSTN_ID"] = _equipmentMeasurementPositionCode;
            dr["LOTID"] = _lotId;
            dr["WIPSEQ"] = _wipSeq;
            dr["CLCT_SEQNO"] = _collectSeq;
            inTable.Rows.Add(dr);

            new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(searchException);
                    }

                    Util.GridSetData(dgList, searchResult, FrameOperation, true);
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    Util.MessageException(ex);
                }
            });
        }

        #endregion


    }
}
