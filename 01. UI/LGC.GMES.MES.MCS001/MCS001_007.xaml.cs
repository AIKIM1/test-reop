/*************************************************************************************
 Created Date : 2018.10.18
      Creator : 오화백
   Decription : 라미대기창고 입출고 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2018.10.18  DEVELOPER : Initial Created.


 
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
    public partial class MCS001_007 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtMain = new DataTable();
        Util _Util = new Util();


        public MCS001_007()
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            //if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            //{
            //    Array ary = FrameOperation.Parameters;
            //    txtLot.Text = ary.GetValue(0).ToString();
            //    GetResult();
            //}
            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// 콤보박스 
        /// </summary>
        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                //Stocker
                String[] sFilter2 = { "","Y","" };
                C1ComboBox[] cboAreaChild = { cboPortID };
                _combo.SetCombo(cboStocker, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild,sFilter: sFilter2, sCase: "CWALAMISTOCKER");
                //PORTID
                C1ComboBox[] cboStockerParent = { cboStocker };
                _combo.SetCombo(cboPortID, CommonCombo.ComboStatus.ALL, cbParent: cboStockerParent,  sCase: "CWALAMIPORT");
                //입출고
                String[] sFilter1 = { "WH_TRAY_STATUS" };
                _combo.SetCombo(cboINOUT, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        #region Event

        #region 버튼 조회 : btnSearch()
        /// <summary>
        /// 버튼 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch(object sender, RoutedEventArgs e)
        {
            GetResult();
        }
        #endregion


        #endregion

        #region Mehod
      
        #region 조회 : GetResult()
        private void GetResult()
        {
            if (Convert.ToDecimal(Convert.ToDateTime(dtpFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(dtpTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.Alert("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }
            string sEqptid = cboStocker.SelectedValue.ToString();
            string sPortid = cboPortID.SelectedValue.ToString();
            string sInout = cboINOUT.SelectedValue.ToString();
            string sRackid = txtRackID.Text;
            string sLotId = txtLot.Text;
            string sDaName = string.Empty;

            if (sEqptid == "")
            {
                sEqptid = null;
            }
            if (sPortid == "")
            {
                sPortid = null;
            }
            if (sRackid == "")
            {
                sRackid = null;
            }
            if (sLotId == "")
            {
                sLotId = null;
            }

            if (sInout.ToString() == "IN")
            {
                sDaName = "DA_MCS_SEL_LAMI_HIST_IN";
            }
            else if (sInout.ToString() == "OUT")
            {
                sDaName = "DA_MCS_SEL_LAMI_HIST_OUT";
            }
            else
            {
                sDaName = "DA_MCS_SEL_LAMI_HIST_ALL";
            }
            DataTable inTable = new DataTable();
            inTable.Columns.Add("FROM_DATE", typeof(string));
            inTable.Columns.Add("TO_DATE", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("PORT_ID", typeof(string));
            inTable.Columns.Add("RACK_ID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("LANGID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["FROM_DATE"] = dtpFrom.SelectedDateTime.ToString("yyyyMMdd");
            newRow["TO_DATE"] = dtpTo.SelectedDateTime.ToString("yyyyMMdd");
            newRow["EQPTID"] = sEqptid;
            newRow["PORT_ID"] = sPortid;
            newRow["RACK_ID"] = sRackid;
            newRow["LOTID"] = sLotId;
            newRow["LANGID"] = LoginInfo.LANGID;
            inTable.Rows.Add(newRow);
            dtMain = new ClientProxy().ExecuteServiceSync(sDaName, "INDATA", "OUTDATA", inTable);
            if(dtMain.Rows.Count>0)
            {
                for(int i=0; i< dtMain.Rows.Count; i++)
                {
                    if (dtMain.Rows[i]["TRSF_TYPE"].ToString() == "IN")
                    {
                        dtMain.Rows[i]["TRSF_TYPE"] =  ObjectDic.Instance.GetObjectName("입고");
                    }
                    else if (dtMain.Rows[i]["TRSF_TYPE"].ToString() == "OUT")
                    {
                        dtMain.Rows[i]["TRSF_TYPE"] = ObjectDic.Instance.GetObjectName("출고");
                    }
                }
            }

         
            Util.GridSetData(dgLotList, dtMain, FrameOperation, true);

        }

        #endregion

        #endregion
    }
}
