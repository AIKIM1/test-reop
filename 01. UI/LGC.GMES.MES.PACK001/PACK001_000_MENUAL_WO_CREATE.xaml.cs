/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack ID발행 화면 - 발행된 라벨ID 표시 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
  2024.04.22  KIM MIN SEOK      [E20240408-000384] BIZ와 날짜 형식 불일치로 인한 형식 일치 작업 및 날짜, 생산 수량 VALIDATION 추가





 
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_000_MENUAL_WO_CREATE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        private System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        string sEqsgID;
        string sProdID;
        string sRoutid;
        string sPcsgID;
        string sProcID;


        private string rsPcsgID;
        public string PCSGID
        {
            get
            {
                return rsPcsgID;
            }

            set
            {
                rsPcsgID = value;
            }
        }

        private string rsEqsgID;
        public string EQSGID
        {
            get
            {
                return rsEqsgID;
            }

            set
            {
                rsEqsgID = value;
            }
        }
        private string rsRoutID;
        public string ROUTID
        {
            get
            {
                return rsRoutID;
            }

            set
            {
                rsRoutID = value;
            }
        }

        private string rsWOID;
        public string WOID
        {
            get
            {
                return rsWOID;
            }

            set
            {
                rsWOID = value;
            }
        }

        private string rsProdID;
        public string PRODID
        {
            get
            {
                return rsProdID;
            }

            set
            {
                rsProdID = value;
            }
        }

        private bool rbWOCheck ;
        public bool WOCHECK
        {
            get
            {
                return rbWOCheck;
            }

            set
            {
                rbWOCheck = value;
            }
        }

        private string sProcid = string.Empty;

        public string Procid
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

        public PACK001_000_MENUAL_WO_CREATE()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            DataRow rows = (DataRow)tmps[1];
            txtPRODID.Text = rows["PRODID"].ToString();
            txtmodel.Text = rows["PRJ_NAME"].ToString();
            DateTime dtFROM = DateTime.Parse(GetSystemTime().ToString("yyyy-MM-01"));
            DateTime dtTo = dtFROM.AddMonths(1);
            dtpDateFrom.SelectedDateTime = dtFROM;
            dtpDateTo.SelectedDateTime = dtTo.AddDays(-1);
            txtCOUNT.Value = 9999;
            sEqsgID = rows["EQSGID"].ToString();
            sProdID = rows["PRODID"].ToString();
            sRoutid = rows["ROUTID"].ToString();
            sPcsgID ="B";
            sProcID = (string)tmps[2];

        }
        private void txtKeypart_After_KeyDown(object sender, KeyEventArgs e)
        {
            
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Mehod

        #endregion
        private void selWorkOrder_DateCheck()
        {
            try
            {
                DateTime dtFROMDTTM = dtpDateFrom.SelectedDateTime;
                DateTime dtTODTTM = dtpDateTo.SelectedDateTime;

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("FROMDTTM", typeof(string));
                INDATA.Columns.Add("TODTTM", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["FROMDTTM"] = dtFROMDTTM.ToString("yyyy-MM-dd");
                dr["TODTTM"] = dtTODTTM.ToString("yyyy-MM-dd");
                INDATA.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_CHK_DATE_FOR_PACK", "INDATA", null, INDATA, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                    }
                    else
                    {
                        woSave();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void woSave()
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("ROUTID", typeof(string));
                //2O24.04.22 BIZ와 형식 불일치로 인한 형식 일치 작업 - KIM MIN SEOK
                INDATA.Columns.Add("ST_DATE", typeof(string));
                INDATA.Columns.Add("ED_DATE", typeof(string));
                INDATA.Columns.Add("PCSGID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("INPUT_QTY", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));

                if(Convert.ToInt32(txtCOUNT.Value) <= 0)
                {
                    ms.AlertWarning("SFU3092");
                    return;
                }

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["EQSGID"] = sEqsgID;
                drINDATA["PRODID"] = sProdID;
                drINDATA["ROUTID"] = sRoutid;
                drINDATA["ST_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                drINDATA["ED_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd");
                drINDATA["PCSGID"] = sPcsgID;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["INPUT_QTY"] = Convert.ToInt32(txtCOUNT.Value);
                drINDATA["PROCID"] = sProcID;
                INDATA.Rows.Add(drINDATA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CREAT_MES_WORK_ORDER", "INDATA", "OUTDATA", INDATA);
                if (dtResult.Rows.Count > 0)
                {
                    WOID = dtResult.Rows[0]["WOID"].ToString();
                    EQSGID = sEqsgID;
                    PRODID = sProdID;
                    ROUTID = sRoutid;
                    PCSGID = sPcsgID;
                    WOCHECK = (bool)chkWOChange.IsChecked;

                    this.DialogResult = MessageBoxResult.OK;
                }
                else
                {
                    this.DialogResult = MessageBoxResult.None;
                }
            }
            catch (Exception ex)
            {
                Util.AlertError(ex);

            }
            finally
            {
                
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                selWorkOrder_DateCheck();
            }
            catch (Exception ex)
            {
                Util.AlertError(ex);

            }

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
    }
}
