/*************************************************************************************
 Created Date : 2018.10.17
      Creator : 오화백
   Decription : 물류장비 이벤트 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2018.10.17  DEVELOPER : Initial Created.


 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_008 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtMain = new DataTable();
        Util _Util = new Util();


        public MCS001_008()
        {
            InitializeComponent();
            InitCombo();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                Array ary = FrameOperation.Parameters;
                txtLot.Text = ary.GetValue(0).ToString();
                GetResult();
            }
           

            this.Loaded -= UserControl_Loaded;
        }
        #endregion


        #region Initialize

        #endregion
        
        #region Event
        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();


                //설비타입
                String[] sFilter1 = { "LOGIS_EQPT_DETL_TYPE" };
                C1ComboBox[] cboAreaChild = { cboEquipment };
                _combo.SetCombo(cboEquipmentType, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild, sFilter: sFilter1, sCase: "COMMCODE");

                //설비명
                C1ComboBox[] cboEquipmentTypeParent = { cboEquipmentType };
                _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentTypeParent, sCase: "CWAMCSEQUIPMENT");

                //설비상태
                String[] sFilter2 = { "LOGIS_EQPT_STAT_CODE" };
                _combo.SetCombo(cboEquipmentStat, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void btnSearch(object sender, RoutedEventArgs e)
        {
            GetResult();
        }
        #endregion

        #region Mehod
        private void GetResult()
        {
            if (Convert.ToDecimal(Convert.ToDateTime(dtpFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(dtpTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.Alert("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }
            string sCmdID = txtcommand.Text.ToString();
            string sEquip = cboEquipment.SelectedValue.ToString();
            string sLotId = txtLot.Text.ToString();
            string sState = cboEquipmentStat.SelectedValue.ToString();
            if (sCmdID == "")
            {
                sCmdID = null;
            }
            if (sEquip == "")
            {
                sEquip = null;
            }
            if (sLotId == "")
            {
                sLotId = null;
            }
         
            if (sState == "")
            {
                sState = null;
            }
            DataTable inTable = new DataTable();
            inTable.Columns.Add("FROM_DATE", typeof(string));
            inTable.Columns.Add("TO_DATE", typeof(string));
            inTable.Columns.Add("LOGIS_CMD_ID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("LOGIS_EQPT_STAT_CODE", typeof(string));
            inTable.Columns.Add("LANGID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["FROM_DATE"] = dtpFrom.SelectedDateTime.ToString("yyyyMMdd");
            newRow["TO_DATE"] = dtpTo.SelectedDateTime.ToString("yyyyMMdd");
            newRow["LOGIS_CMD_ID"] = sCmdID;
            newRow["EQPTID"] = sEquip;
            newRow["LOTID"] = sLotId;
            newRow["LOGIS_EQPT_STAT_CODE"] = sState;
            newRow["LANGID"] = LoginInfo.LANGID;

            inTable.Rows.Add(newRow);

            dtMain = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_DISTRIBUTION_EVENT_HIST", "INDATA", "OUTDATA", inTable);

            Util.GridSetData(dgHistList, dtMain, FrameOperation, true);
                 
        }
        #endregion
    }
}
