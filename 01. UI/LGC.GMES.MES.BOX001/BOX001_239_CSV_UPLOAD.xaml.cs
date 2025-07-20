/*************************************************************************************
 Created Date : 2024.08.12
      Creator : 이병윤
   Decription : E20240717-000944 : 업로더 처리
--------------------------------------------------------------------------------------
 [Change History]
 
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid.Summaries;

using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_240_NONIMOUTBOX.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_239_CSV_UPLOAD : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        Util _util = new Util();
        string sPrt = string.Empty;
        string sRes = string.Empty;
        string sCopy = string.Empty;
        string sXpos = string.Empty;
        string sYpos = string.Empty;
        string sDark = string.Empty;
        DataRow drPrtInfo = null;

        string _AREAID = string.Empty;
        string sUSERID = string.Empty;
        string sSHFTID = string.Empty;
        string sPGMID  = string.Empty;
        string sBoxId  = string.Empty;
        string sZplCd  = string.Empty;
        DataTable dtRqst = new DataTable();

        public string AREAID
        {
            get;
            set;
        }
        public string PALLETID
        {
            get;
            set;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_239_CSV_UPLOAD()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                dtRqst = tmps[0] as DataTable;

                Util.GridSetData(dgResult, dtRqst, FrameOperation);
                DataGridAggregate.SetAggregateFunctions(dgResult.Columns["LABEL_6J_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = string.Empty } });
                DataGridAggregatesCollection dac = new DataGridAggregatesCollection();
                DataGridAggregateSum dagsum = new DataGridAggregateSum();
                dagsum.ResultTemplate = dgResult.Resources["ResultTemplate"] as DataTemplate;
                dac.Add(dagsum);
                DataGridAggregate.SetAggregateFunctions(dgResult.Columns["LABEL_6J_QTY"], dac);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
        }
        #endregion

        #region [Events]
        /// <summary>
        /// Print 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            // 업로드하시겟습니까? 
            Util.MessageConfirm("SFU9932", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        
        #endregion

        #region [Method]
        /// <summary>
        /// 생성 Outbox 조회
        /// </summary>
        private void Save()
        {
            try
            {
                // 라벨타입조회
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PLLT", typeof(string));

                DataTable dtSource = DataTableConverter.Convert(dgResult.ItemsSource);

                RQSTDT.Merge(dtSource);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_SHIPMENTNO_6J_LABEL", "INDATA", "OUTDATA", RQSTDT);
                Util.MessageInfo("SFU1270");

                Util.gridClear(dgResult);

                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
