/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : 공정의 작업정보
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.





 
**************************************************************************************/
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_001_PROCESSINFO : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public PACK001_001 PACK001_001;
        public PACK001_004 PACK001_004;
        public PACK001_002 PACK001_002;

        private System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();

        ///// <summary>
        ///// COLUMNS:LINEID, LINENAME, PROCID, PROCNAME, PROCSEQ, EQPTID, EQPTNAME
        ///// </summary>
        private DataRow drPROCESSINFO = null;

        private string sLineid = string.Empty;
        private string sLineName = string.Empty;
        private string sPcsgid = string.Empty;
        private string sProcid = string.Empty;
        private string sProcName = string.Empty;
        private string sEqptid = string.Empty;
        private string sEqptName = string.Empty;
        private string sProductid = string.Empty;
        private string sProductName = string.Empty;
        private string sWorkorderid = string.Empty;
        private string sRouteid = string.Empty;
        private string sFrowid = string.Empty;
        private DataTable dtWO_MTRL_info = new DataTable();

        public string EQSGID
        {
            get
            {
                return sLineid;
            }

            set
            {
                sLineid = value;
            }
        }

        public string EQSGNAME
        {
            get
            {
                return sLineName;
            }

            set
            {
                sLineName = value;
            }
        }

        public string PROCID
        {
            get
            {
                return sProcid;
            }

            set
            {
                sProcid = value;
            }
        }

        public string PROCNAME
        {
            get
            {
                return sProcName;
            }

            set
            {
                sProcName = value;
            }
        }

        public string EQPTID
        {
            get
            {
                return sEqptid;
            }

            set
            {
                sEqptid = value;
            }
        }

        public string EQPTNAME
        {
            get
            {
                return sEqptName;
            }

            set
            {
                sEqptName = value;
            }
        }

        public string PRODUCTID
        {
            get
            {
                return sProductid;
            }

            set
            {
                sProductid = value;
            }
        }

        public string PRODUCTNAME
        {
            get
            {
                return sProductName;
            }

            set
            {
                sProductName = value;
            }
        }

        public string WORKORDER
        {
            get
            {
                return sWorkorderid;
            }

            set
            {
                sWorkorderid = value;
            }
        }

        public string ROUTID
        {
            get
            {
                return sRouteid;
            }

            set
            {
                sRouteid = value;
            }
        }

        public string FLOWID
        {
            get
            {
                return sFrowid;
            }

            set
            {
                sFrowid = value;
            }
        }

        public string PCSGID
        {
            get
            {
                return sPcsgid;
            }

            set
            {
                sPcsgid = value;
            }
        }

        /// <summary>
        ///PRODID, PRODNAME, WOID, ROUTID, FLOWID, PCSGID, INPUT_PROCID, MTRLID, PROC_INPUT_QTY
        /// </summary>
        public DataTable WO_MTRL_INFO
        {
            get
            {
                return dtWO_MTRL_info;
            }

            set
            {
                dtWO_MTRL_info = value;
            }
        }

        public PACK001_001_PROCESSINFO()
        {
            InitializeComponent();
        }



        #endregion

        #region Initialize

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
            try
            {
                //TimerSetting();
                //timer.IsEnabled = true;
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            try
            {
                setPlanQty();
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {   
            try
            {
                timer.IsEnabled = false;
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                setPlanQty();
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        

        private void btnMobomPopUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PACK001_001_MBOMINFO popup = new PACK001_001_MBOMINFO();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("EQSGID", typeof(string));
                    dtData.Columns.Add("EQSGNAME", typeof(string));
                    dtData.Columns.Add("PROCID", typeof(string));
                    dtData.Columns.Add("PROCNAME", typeof(string));
                    dtData.Columns.Add("PRODID", typeof(string));

                    DataRow newRow = null;

                    newRow = dtData.NewRow();
                    newRow["EQSGID"] = EQSGID;
                    newRow["EQSGNAME"] = EQSGNAME;
                    newRow["PROCID"] = PROCID;
                    newRow["PROCNAME"] = PROCNAME;
                    newRow["PRODID"] = PRODUCTID;

                    dtData.Rows.Add(newRow);

                    //========================================================================
                    object[] Parameters = new object[1];
                    Parameters[0] = dtData;
                    C1WindowExtension.SetParameters(popup, Parameters);
                    //========================================================================

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void txtSelectedProduct_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (PRODUCTID.Length > 0)
                {
                    btnMobomPopUp.IsEnabled = true;
                }
                else
                {
                    btnMobomPopUp.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        void popup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_001_MBOMINFO popup = sender as PACK001_001_MBOMINFO;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }

        }

        #endregion

        #region Mehod


        private void TimerSetting()
        {
            timer.Interval = new TimeSpan(0, 0, 0, 3);
            timer.Tick += new EventHandler(timer_Tick);
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            setPlanQty();
        }
        public DataTable setPlanQty()
        {
            DataTable dtResult = null;
            try
            {       
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.NVC(EQSGID);
                dr["PROCID"] = Util.NVC(PROCID);
                dr["EQPTID"] = Util.NVC(EQPTID);
                RQSTDT.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODPLAN_RESULT_BY_EQSG", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    txtCaldate.Text = Util.NVC(dtResult.Rows[0]["CALDATE_DATE"]);
                    txtShift.Text = Util.NVC(dtResult.Rows[0]["SHFT_NAME"]);
                    //txtPlanQty.Text = Util.NVC(dtResult.Rows[0]["PLANQTY"]);
                    txtGoodQty.Text = Util.NVC(dtResult.Rows[0]["GOODQTY"]);
                    txtDefectQty.Text = Util.NVC(dtResult.Rows[0]["DEFECTQTY"]);
                }
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_PRODPLAN_RESULT_BY_EQSG", ex.Message, ex.ToString());
                Util.MessageException(ex);
                return dtResult;
            }
            return dtResult;
        }
        
        public void setProcess(DataRow drProcessInfo)
        {
            try
            {

                EQSGID = string.Empty;
                EQSGNAME = string.Empty;
                EQPTID = string.Empty;
                EQPTNAME = string.Empty;
                PROCID = string.Empty;
                PROCNAME = string.Empty;
                PRODUCTID = string.Empty;
                PRODUCTNAME = string.Empty;
                WORKORDER = string.Empty;
                ROUTID = string.Empty;
                FLOWID = string.Empty;
                PCSGID = string.Empty;
                WO_MTRL_INFO = null;

                EQSGID = Util.NVC(drProcessInfo["EQSGID"]);
                EQSGNAME = Util.NVC(drProcessInfo["EQSGNAME"]);
                EQPTID = Util.NVC(drProcessInfo["EQPTID"]);
                EQPTNAME = Util.NVC(drProcessInfo["EQPTNAME"]);
                PROCID = Util.NVC(drProcessInfo["PROCID"]);
                PROCNAME = Util.NVC(drProcessInfo["PROCNAME"]);
                string sRoutID_Temp = Util.NVC(drProcessInfo["PROC_ROUTID"]);

                //공정ID로 작지선택및 라우트정보 조회.
                //DataTable dtWoInfo = getSelectedWoInfo(EQSGID, PROCID);
                DataTable dtWoInfo = getSelectedWoInfo(EQSGID, PROCID, sRoutID_Temp);
                
                if (dtWoInfo != null)
                {
                    if (dtWoInfo.Rows.Count > 0)
                    {
                        PRODUCTID = Util.NVC(dtWoInfo.Rows[0]["PRODID"]);
                        PRODUCTNAME = Util.NVC(dtWoInfo.Rows[0]["PRODNAME"]);
                        WORKORDER = Util.NVC(dtWoInfo.Rows[0]["WOID"]);
                        ROUTID = Util.NVC(dtWoInfo.Rows[0]["ROUTID"]);
                        FLOWID = Util.NVC(dtWoInfo.Rows[0]["FLOWID"]);
                        PCSGID = Util.NVC(dtWoInfo.Rows[0]["PCSGID"]);

                        WO_MTRL_INFO = dtWoInfo.Copy();
                    }
                }

                txtSelectedLine.Text = EQSGNAME;
                txtSelectedProcess.Text = PROCNAME;
                txtSelectedEquipment.Text = EQPTNAME;
                txtSelectedProduct.Text = PRODUCTID;
                txtSelectedWorkOrder.Text = WORKORDER;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 공정의 선택작업지시,제품,라우트,FLOW 등 조회
        /// </summary>
        /// <param name="sProcid"></param>
        /// <returns></returns>
        private DataTable getSelectedWoInfo(string sEqsgID,string sProcid)
        {
            DataTable dtReturn = null;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sProcid;
                dr["EQSGID"] = sEqsgID;
                RQSTDT.Rows.Add(dr);

                dtReturn = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_PCSG_WO", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch(Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_TB_SFC_PCSG_WO", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
            return dtReturn;
        }

        private DataTable getSelectedWoInfo(string sEqsgID, string sProcid, string sRouteid)
        {
            DataTable dtReturn = null;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sProcid;
                dr["EQSGID"] = sEqsgID;
                dr["PRODID"] = null;
                dr["ROUTID"] = sRouteid;
                RQSTDT.Rows.Add(dr);

                dtReturn = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ROUT_WO_PACK", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_ROUT_WO_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
            return dtReturn;
        }



        #endregion


    }
}
